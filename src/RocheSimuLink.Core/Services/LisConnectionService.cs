using RocheSimuLink.HL7.Parsers;
using RocheSimuLink.HL7.Transport;
using RocheSimuLink.Logging;
using RocheSimuLink.Models;
using RocheSimuLink.Models.Orders;

namespace RocheSimuLink.Services
{
    /// <summary>Connection lifecycle states surfaced to the UI.</summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    /// <summary>
    /// Coordinates the MLLP client (outbound results) and listener (inbound
    /// orders), exposes connection state, and records activity to the log.
    /// The UI binds Connect/Disconnect/Send to this service.
    /// </summary>
    public sealed class LisConnectionService : IDisposable
    {
        private readonly ConnectionSettings _settings;
        private readonly ActivityLog _log;
        private MllpClient? _client;
        private MllpListener? _listener;

        public LisConnectionService(ConnectionSettings settings, ActivityLog log)
        {
            _settings = settings;
            _log = log;
        }

        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

        /// <summary>
        /// The port the inbound-order listener is bound to. Reflects the
        /// OS-assigned port when configured with 0. Zero when not listening.
        /// </summary>
        public int ListenPort => _listener?.Port ?? 0;

        /// <summary>Raised when <see cref="State"/> changes.</summary>
        public event EventHandler<ConnectionState>? StateChanged;

        /// <summary>Raised when an order is received and parsed from the LIS.</summary>
        public event EventHandler<ReceivedOrder>? OrderReceived;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (State == ConnectionState.Connected)
            {
                return;
            }

            SetState(ConnectionState.Connecting);

            try
            {
                _client = new MllpClient(_settings.LisHost, _settings.LisPort);
                await _client.ConnectAsync(cancellationToken).ConfigureAwait(false);

                _listener = new MllpListener(_settings.ListenPort);
                _listener.MessageReceived += OnMessageReceived;
                _listener.Error += (_, ex) => _log.Error($"Listener error: {ex.Message}");
                _listener.Start();

                SetState(ConnectionState.Connected);
                _log.Success($"Connected to LIS server ({_settings.LisHost}:{_settings.LisPort}); listening on port {_listener.Port}.");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to connect to LIS: {ex.Message}");
                await DisconnectAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_listener is not null)
            {
                _listener.MessageReceived -= OnMessageReceived;
                await _listener.StopAsync().ConfigureAwait(false);
                _listener.Dispose();
                _listener = null;
            }

            _client?.Dispose();
            _client = null;

            if (State != ConnectionState.Disconnected)
            {
                SetState(ConnectionState.Disconnected);
                _log.Info("Disconnected from LIS.");
            }
        }

        /// <summary>
        /// Sends an HL7 result message to the LIS and returns the acknowledgement.
        /// </summary>
        public async Task<string> SendResultAsync(string hl7Message, CancellationToken cancellationToken = default)
        {
            if (_client is null || State != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Not connected to a LIS.");
            }

            var ack = await _client.SendAsync(hl7Message, cancellationToken).ConfigureAwait(false);
            return ack;
        }

        private void OnMessageReceived(object? sender, MllpMessageReceivedEventArgs e)
        {
            try
            {
                var order = OrderParser.ToOrder(e.Parsed);
                order.RawMessage = e.RawMessage;

                var testSummary = order.Tests.Count > 0
                    ? string.Join(", ", order.Tests.Select(t => $"{t.TestCode} {t.TestName}".Trim()))
                    : "(no tests)";
                _log.Success($"Order received from LIS: Sample ID {order.SampleId}, Test: {testSummary}");

                OrderReceived?.Invoke(this, order);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to parse inbound order: {ex.Message}");
            }
        }

        private void SetState(ConnectionState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(this, state);
        }

        public void Dispose() => DisconnectAsync().GetAwaiter().GetResult();
    }
}
