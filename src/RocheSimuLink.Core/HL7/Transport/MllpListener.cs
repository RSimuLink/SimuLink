using System.Net;
using System.Net.Sockets;
using System.Text;
using RocheSimuLink.HL7.Parsers;

namespace RocheSimuLink.HL7.Transport
{
    /// <summary>
    /// MLLP listener: accepts TCP connections from a LIS, reads MLLP-framed HL7
    /// messages, raises <see cref="MessageReceived"/> for each, and sends back
    /// the acknowledgement the handler supplies (or a default ACK).
    /// </summary>
    public sealed class MllpListener : IDisposable
    {
        private readonly IPAddress _address;
        private readonly Encoding _encoding;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptLoop;

        public MllpListener(int port, IPAddress? address = null, Encoding? encoding = null)
        {
            Port = port;
            _address = address ?? IPAddress.Loopback;
            _encoding = encoding ?? Encoding.UTF8;
        }

        /// <summary>Port the listener binds to. Reflects the OS-assigned port when started with 0.</summary>
        public int Port { get; private set; }

        public bool IsRunning => _listener is not null;

        /// <summary>Raised for each complete inbound HL7 message.</summary>
        public event EventHandler<MllpMessageReceivedEventArgs>? MessageReceived;

        /// <summary>Raised when a connection handler throws.</summary>
        public event EventHandler<Exception>? Error;

        public void Start()
        {
            if (_listener is not null)
            {
                return;
            }

            _listener = new TcpListener(_address, Port);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            _cts = new CancellationTokenSource();
            _acceptLoop = AcceptLoopAsync(_cts.Token);
        }

        public async Task StopAsync()
        {
            if (_listener is null)
            {
                return;
            }

            _cts?.Cancel();
            _listener.Stop();

            if (_acceptLoop is not null)
            {
                try
                {
                    await _acceptLoop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // expected on shutdown
                }
            }

            _listener = null;
            _cts?.Dispose();
            _cts = null;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener!.AcceptTcpClientAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                _ = HandleClientAsync(client, token);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    var buffer = new byte[4096];
                    var accumulated = new List<byte>();

                    while (!token.IsCancellationRequested)
                    {
                        var read = await stream.ReadAsync(buffer, token).ConfigureAwait(false);
                        if (read == 0)
                        {
                            break;
                        }

                        accumulated.AddRange(buffer.AsSpan(0, read).ToArray());

                        // Drain every complete frame currently in the buffer.
                        while (true)
                        {
                            var current = accumulated.ToArray();
                            if (!MllpProtocol.TryReadFrame(current, current.Length, out var payload, out var consumed, _encoding))
                            {
                                break;
                            }

                            accumulated.RemoveRange(0, consumed);

                            var ack = DispatchMessage(payload);
                            var ackFrame = MllpProtocol.Encode(ack, _encoding);
                            await stream.WriteAsync(ackFrame, token).ConfigureAwait(false);
                            await stream.FlushAsync(token).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
            }
        }

        private string DispatchMessage(string payload)
        {
            ParsedHl7Message parsed;
            try
            {
                parsed = Hl7Parser.Parse(payload);
            }
            catch (FormatException)
            {
                return BuildDefaultAck(controlId: string.Empty, code: "AR");
            }

            var args = new MllpMessageReceivedEventArgs(payload, parsed);
            MessageReceived?.Invoke(this, args);

            if (!string.IsNullOrEmpty(args.Acknowledgement))
            {
                return args.Acknowledgement;
            }

            var controlId = parsed.Segment("MSH")?.Field(10) ?? string.Empty;
            return BuildDefaultAck(controlId, code: "AA");
        }

        private static string BuildDefaultAck(string controlId, string code)
        {
            var msh = $"MSH|^~\\&|SimuLink|Roche|LIS|Hospital|{DateTime.Now:yyyyMMddHHmmss}||ACK|{Guid.NewGuid()}|P|2.5.1";
            var msa = $"MSA|{code}|{controlId}";
            return string.Join("\r", msh, msa);
        }

        public void Dispose()
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // best-effort cleanup
            }
        }
    }
}
