using System.Net;
using System.Net.Sockets;
using RocheSimuLink.HL7.Transport;
using RocheSimuLink.Logging;
using RocheSimuLink.Models;
using RocheSimuLink.Models.Orders;
using RocheSimuLink.Services;
using Xunit;

namespace RocheSimuLink.Core.Tests.Services;

public class LisConnectionServiceTests
{
    private const string Order =
        "MSH|^~\\&|LIS|Hospital|SimuLink|Roche|20260624120000||OML^O33|CTRL-7|P|2.5.1\r" +
        "PID|1||789456123^^^LIS||Johnson^Emily||19850825|F\r" +
        "ORC|NW|123987654|||||R\r" +
        "SPM|1|789456123||PLAS\r" +
        "OBR|1|789456123||HPV^HPV Typing^L";

    /// <summary>A stand-in LIS that accepts one connection and ACKs each frame.</summary>
    private static (TcpListener listener, int port) StartFakeLis()
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
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port, ListenPort = 0 };
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
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port, ListenPort = 0 };
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
        var (fakeLis, port) = StartFakeLis();
        var log = new ActivityLog();
        var settings = new ConnectionSettings { LisHost = "127.0.0.1", LisPort = port, ListenPort = 0 };
        using var service = new LisConnectionService(settings, log);

        ReceivedOrder? received = null;
        service.OrderReceived += (_, o) => received = o;

        await service.ConnectAsync();

        // The service is listening on an OS-assigned port; send an order to it
        // the way a LIS would.
        using var lisClient = new MllpClient("127.0.0.1", service.ListenPort);
        await lisClient.SendAsync(Order);

        // Allow the listener's async handler to run.
        for (var i = 0; i < 50 && received is null; i++)
        {
            await Task.Delay(20);
        }

        Assert.NotNull(received);
        Assert.Equal("789456123", received!.SampleId);
        Assert.Equal("Emily Johnson", received.Patient.FullName);
        Assert.Single(received.Tests);
        Assert.Contains(log.Entries, e => e.Message.Contains("Order received"));

        await service.DisconnectAsync();
        fakeLis.Stop();
    }
}
