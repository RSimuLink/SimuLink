using System.Net;
using System.Net.Sockets;
using RocheLIT.HL7.Transport;
using RocheLIT.Logging;
using RocheLIT.Models;
using RocheLIT.Models.Orders;
using RocheLIT.Services;
using Xunit;

namespace RocheLIT.Core.Tests.Services;

public class LisConnectionServiceTests
{
    private const string Order =
        "MSH|^~\\&|LIS|Hospital|LIT|Roche|20260624120000||OML^O33^OML_O33|CTRL-7|P|2.5.1|||NE|AL||UNICODE UTF-8|||LAB-28^IHE\r" +
        "PID|1||789456123^^^LIS||Johnson^Emily||19850825|F\r" +
        "SPM|1|789456123||PLAS^plasma^HL70487|||||||P^^HL70369\r" +
        "SAC|||789456123|||||||1897|5\r" +
        "ORC|NW||||||||20260624120000\r" +
        "OBR||789456123||HPV^HPV Typing^L\r" +
        "TCD|HPV^HPV Typing^L||||||||500^uL&&UCUM";

    /// <summary>A stand-in LIS that accepts one connection and ACKs each frame.</summary>
    private static (TcpListener listener, int port) StartFakeLis(string? messageToSendOnConnect = null)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        _ = Task.Run(async () =>
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();

                if (!string.IsNullOrEmpty(messageToSendOnConnect))
                {
                    await stream.WriteAsync(MllpProtocol.Encode(messageToSendOnConnect));
                    await stream.FlushAsync();
                }

                var buffer = new byte[4096];
                var acc = new List<byte>();
                while (true)
                {
                    var read = await stream.ReadAsync(buffer);
                    if (read == 0) break;
                    acc.AddRange(buffer.AsSpan(0, read).ToArray());
                    var cur = acc.ToArray();
                    if (MllpProtocol.TryReadFrame(cur, cur.Length, out _, out var consumed))
                    {
                        acc.RemoveRange(0, consumed);
                        var ack = MllpProtocol.Encode("MSH|^~\\&|LIS|H|S|R|20260624120000||ACK|1|P|2.5.1\rMSA|AA|CTRL-7");
                        await stream.WriteAsync(ack);
                        await stream.FlushAsync();
                    }
                }
            }
            catch
            {
                // test teardown
            }
        });

        return (listener, port);
    }

    [Fact]
    public async Task Connect_TransitionsToConnected_AndLogsSuccess()
    {
        var (fakeLis, port) = StartFakeLis();
        var log = new ActivityLog();
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port };
        using var service = new LisConnectionService(settings, log);

        var states = new List<ConnectionState>();
        service.StateChanged += (_, s) => states.Add(s);

        await service.ConnectAsync();

        Assert.Equal(ConnectionState.Connected, service.State);
        Assert.Contains(ConnectionState.Connecting, states);
        Assert.Contains(ConnectionState.Connected, states);
        Assert.Contains(log.Entries, e => e.Severity == LogSeverity.Success);

        await service.DisconnectAsync();
        fakeLis.Stop();
    }

    [Fact]
    public async Task SendResult_ReturnsAck()
    {
        var (fakeLis, port) = StartFakeLis();
        var log = new ActivityLog();
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port };
        using var service = new LisConnectionService(settings, log);
        await service.ConnectAsync();

        var ack = await service.SendResultAsync("MSH|^~\\&|S|R|L|H|20260624120000||OUL^R22|1|P|2.5.1\rOBX|1|ST|X||1");

        Assert.Contains("MSA|AA|CTRL-7", ack);

        await service.DisconnectAsync();
        fakeLis.Stop();
    }

    [Fact]
    public async Task SendResult_ThrowsWhenNotConnected()
    {
        var settings = new ConnectionSettings();
        using var service = new LisConnectionService(settings, new ActivityLog());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendResultAsync("X"));
    }

    [Fact]
    public async Task IncomingOrder_RaisesOrderReceived_AndLogs()
    {
        var (fakeLis, port) = StartFakeLis(Order);
        var log = new ActivityLog();
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port };
        using var service = new LisConnectionService(settings, log);

        ReceivedOrder? received = null;
        service.OrderReceived += (_, o) => received = o;

        await service.ConnectAsync();

        // Allow the connection receive loop to dispatch the LIS-sent order.
        for (var i = 0; i < 50 && received is null; i++)
        {
            await Task.Delay(20);
        }

        Assert.NotNull(received);
        Assert.Equal("789456123", received!.SampleId);
        Assert.Equal("HPV Typing", received.TestType);
        Assert.Equal("PLAS^plasma^HL70487", received.SampleType);
        Assert.Equal("500^uL&&UCUM", received.SampleVolume);
        Assert.Equal("1897", received.CarrierId);
        Assert.Equal("5", received.CarrierPosition);
        Assert.Single(received.Tests);
        Assert.Contains(log.Entries, e => e.Message.Contains("Order received"));

        await service.DisconnectAsync();
        fakeLis.Stop();
    }

    [Fact]
    public async Task IncomingInvalidOrder_LogsValidationError_AndDoesNotRaiseOrderReceived()
    {
        var invalidOrder = Order.Replace(
            "SAC|||789456123|||||||1897|5",
            "SAC|||789456123|||||1897|5",
            StringComparison.Ordinal);
        var (fakeLis, port) = StartFakeLis(invalidOrder);
        var log = new ActivityLog();
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port };
        using var service = new LisConnectionService(settings, log);

        ReceivedOrder? received = null;
        service.OrderReceived += (_, o) => received = o;

        await service.ConnectAsync();

        for (var i = 0; i < 50 && !log.Entries.Any(e => e.Severity == LogSeverity.Error); i++)
        {
            await Task.Delay(20);
        }

        Assert.Null(received);
        Assert.Contains(log.Entries, e =>
            e.Severity == LogSeverity.Error &&
            e.Message.Contains("Inbound HL7 validation error: SAC[1]-10"));

        await service.DisconnectAsync();
        fakeLis.Stop();
    }
}
