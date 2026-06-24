using System.Net.Sockets;
using System.Text;

namespace RocheSimuLink.HL7.Transport
{
    /// <summary>
    /// MLLP client: connects to a LIS over TCP, sends an HL7 message framed in
    /// MLLP, and reads the acknowledgement frame.
    /// </summary>
    public sealed class MllpClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly Encoding _encoding;
        private TcpClient? _tcp;

        public MllpClient(string host, int port, Encoding? encoding = null)
        {
            _host = host;
            _port = port;
            _encoding = encoding ?? Encoding.UTF8;
        }

        public bool IsConnected => _tcp?.Connected ?? false;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);
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

            var stream = _tcp!.GetStream();
            var frame = MllpProtocol.Encode(hl7Message, _encoding);
            await stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

            return await ReadFrameAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            var accumulated = new List<byte>();

            while (true)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    throw new IOException("Connection closed before a complete MLLP frame was received.");
                }

                accumulated.AddRange(buffer.AsSpan(0, read).ToArray());
                var current = accumulated.ToArray();

                if (MllpProtocol.TryReadFrame(current, current.Length, out var payload, out _, _encoding))
                {
                    return payload;
                }
            }
        }

        public void Dispose()
        {
            _tcp?.Dispose();
            _tcp = null;
        }
    }
}
