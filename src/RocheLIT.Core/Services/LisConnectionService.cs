using RocheLIT.HL7.Parsers;
using RocheLIT.HL7.Transport;
using RocheLIT.HL7.Validation;
using RocheLIT.Logging;
using RocheLIT.Models;
using RocheLIT.Models.Orders;

namespace RocheLIT.Services
{
    /// <summary>Connection lifecycle states surfaced to the UI.</summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    /// <summary>
    /// Coordinates the MLLP connection to the LIS, exposes connection state,
    /// routes inbound orders received on that connection, and records activity
    /// to the log. The UI binds Connect/Disconnect/Send to this service.
    /// </summary>
    public sealed class LisConnectionService : IDisposable
    {
        private readonly ConnectionSettings _settings;
        private readonly ActivityLog _log;
        private MllpClient? _client;

        public LisConnectionService(ConnectionSettings settings, ActivityLog log)
        {
            _settings = settings;
            _log = log;
        }

        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

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
                _client.MessageReceived += OnMessageReceived;
                _client.Error += (_, ex) => _log.Error($"LIS connection error: {ex.Message}");
                await _client.ConnectAsync(cancellationToken).ConfigureAwait(false);

                SetState(ConnectionState.Connected);
                _log.Success(
                    $"Connected to LIS server ({_settings.LisHost}:{_settings.LisPort}); " +
                    "receiving LIS orders over the active connection.");
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
            if (_client is not null)
            {
                _client.MessageReceived -= OnMessageReceived;
                _client.Dispose();
                _client = null;
            }
            await Task.CompletedTask.ConfigureAwait(false);

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
                var validationIssues = Hl7OrderValidator.Validate(e.Parsed);
                if (validationIssues.Count > 0)
                {
                    foreach (var issue in validationIssues)
                    {
                        _log.Error($"Inbound HL7 validation error: {issue}");
                    }

                    e.Acknowledgement = BuildApplicationRejectAck(e.Parsed);
                    return;
                }

                var order = OrderParser.ToOrder(e.Parsed);

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

        private static string BuildApplicationRejectAck(ParsedHl7Message parsed)
        {
            var controlId = parsed.Segment("MSH")?.Field(10) ?? string.Empty;
            return string.Join("\r",
                $"MSH|^~\\&|LIT|Roche|LIS|Hospital|{DateTime.Now:yyyyMMddHHmmss}||ACK|{Guid.NewGuid()}|P|2.5.1",
                $"MSA|AR|{controlId}");
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
