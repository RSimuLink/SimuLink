using System.Net.Sockets;
using System.Text;
using RocheLIT.HL7.Parsers;

namespace RocheLIT.HL7.Transport
{
    /// <summary>
    /// MLLP client: connects to a LIS over TCP, sends HL7 messages framed in
    /// MLLP, receives acknowledgements, and dispatches unsolicited inbound
    /// messages that arrive on the same connection.
    /// </summary>
    public sealed class MllpClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly Encoding _encoding;
        private readonly Queue<TaskCompletionSource<string>> _pendingAcks = new();
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private TcpClient? _tcp;
        private NetworkStream? _stream;
        private CancellationTokenSource? _receiveCts;
        private Task? _receiveLoop;

        public MllpClient(string host, int port, Encoding? encoding = null)
        {
            _host = host;
            _port = port;
            _encoding = encoding ?? Encoding.UTF8;
        }

        public bool IsConnected => _tcp?.Connected ?? false;

        /// <summary>Raised for each non-ACK inbound HL7 message.</summary>
        public event EventHandler<MllpMessageReceivedEventArgs>? MessageReceived;

        /// <summary>Raised when the connection receive loop cannot continue.</summary>
        public event EventHandler<Exception>? Error;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_tcp?.Connected == true)
            {
                return;
            }

            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);
            _stream = _tcp.GetStream();
            _receiveCts = new CancellationTokenSource();
            _receiveLoop = ReceiveLoopAsync(_receiveCts.Token);
        }

        /// <summary>
        /// Sends an HL7 message and returns the acknowledgement payload.
        /// Connects automatically if not already connected.
        /// </summary>
        public async Task<string> SendAsync(string hl7Message, CancellationToken cancellationToken = default)
        {
            if (_tcp is null || !_tcp.Connected)
            {
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
            }

            var ack = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_pendingAcks)
            {
                _pendingAcks.Enqueue(ack);
            }

            try
            {
                await SendFrameAsync(hl7Message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ack.TrySetException(ex);
                throw;
            }

            using var registration = cancellationToken.Register(() =>
                ack.TrySetCanceled(cancellationToken));
            return await ack.Task.ConfigureAwait(false);
        }

        private async Task SendFrameAsync(string hl7Message, CancellationToken cancellationToken)
        {
            if (_stream is null)
            {
                throw new InvalidOperationException("Not connected to a LIS.");
            }

            var frame = MllpProtocol.Encode(hl7Message, _encoding);
            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
                await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            var accumulated = new List<byte>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var read = await _stream!.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
                        throw new IOException("The LIS connection was closed.");
                    }

                    accumulated.AddRange(buffer.AsSpan(0, read).ToArray());

                    while (true)
                    {
                        var current = accumulated.ToArray();
                        if (!MllpProtocol.TryReadFrame(current, current.Length, out var payload, out var consumed, _encoding))
                        {
                            break;
                        }

                        accumulated.RemoveRange(0, consumed);
                        await DispatchFrameAsync(payload, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (ObjectDisposedException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                FailPendingAcks(ex);
                Error?.Invoke(this, ex);
            }
        }

        private async Task DispatchFrameAsync(string payload, CancellationToken cancellationToken)
        {
            ParsedHl7Message parsed;
            try
            {
                parsed = Hl7Parser.Parse(payload);
            }
            catch (FormatException)
            {
                await SendFrameAsync(BuildDefaultAck(controlId: string.Empty, code: "AR"), cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            if (IsAcknowledgement(parsed))
            {
                CompleteNextAck(payload);
                return;
            }

            var args = new MllpMessageReceivedEventArgs(payload, parsed);
            MessageReceived?.Invoke(this, args);

            var ack = !string.IsNullOrEmpty(args.Acknowledgement)
                ? args.Acknowledgement
                : BuildDefaultAck(parsed.Segment("MSH")?.Field(10) ?? string.Empty, code: "AA");
            await SendFrameAsync(ack, cancellationToken).ConfigureAwait(false);
        }

        private static bool IsAcknowledgement(ParsedHl7Message parsed) =>
            parsed.MessageType.StartsWith("ACK", StringComparison.OrdinalIgnoreCase) ||
            parsed.Segment("MSA") is not null;

        private void CompleteNextAck(string payload)
        {
            while (true)
            {
                TaskCompletionSource<string>? ack;
                lock (_pendingAcks)
                {
                    if (!_pendingAcks.TryDequeue(out ack))
                    {
                        return;
                    }
                }

                if (ack.TrySetResult(payload))
                {
                    return;
                }
            }
        }

        private void FailPendingAcks(Exception ex)
        {
            lock (_pendingAcks)
            {
                while (_pendingAcks.TryDequeue(out var ack))
                {
                    ack.TrySetException(ex);
                }
            }
        }

        private static string BuildDefaultAck(string controlId, string code)
        {
            var msh = $"MSH|^~\\&|LIT|Roche|LIS|Hospital|{DateTime.Now:yyyyMMddHHmmss}||ACK|{Guid.NewGuid()}|P|2.5.1";
            var msa = $"MSA|{code}|{controlId}";
            return string.Join("\r", msh, msa);
        }

        public void Dispose()
        {
            try
            {
                _receiveCts?.Cancel();
            }
            catch
            {
                // best-effort cleanup
            }

            _tcp?.Dispose();
            _receiveCts?.Dispose();
            _writeLock.Dispose();
            _stream = null;
            _tcp = null;
            _receiveCts = null;
            _receiveLoop = null;
        }
    }
}
