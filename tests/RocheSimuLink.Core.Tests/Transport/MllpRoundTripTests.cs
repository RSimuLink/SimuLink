using RocheSimuLink.HL7.Parsers;
using RocheSimuLink.HL7.Transport;
using Xunit;

namespace RocheSimuLink.Core.Tests.Transport;

public class MllpRoundTripTests
{
    private const string Order =
        "MSH|^~\\&|LIS|Hospital|SimuLink|Roche|20260624120000||OML^O33|CTRL-42|P|2.5.1\r" +
        "PID|1||SID999^^^LIS||Smith^Jane\r" +
        "OBR|1|SID999||GLU^Glucose^L";

    [Fact]
    public async Task ClientSend_ListenerReceivesParsedMessage_AndReturnsDefaultAck()
    {
        using var listener = new MllpListener(port: 0);
        ParsedHl7Message? received = null;
        listener.MessageReceived += (_, e) => received = e.Parsed;
        listener.Start();

        using var client = new MllpClient("127.0.0.1", listener.Port);
        var ack = await client.SendAsync(Order);

        Assert.NotNull(received);
        Assert.Equal("OML^O33", received!.MessageType);
        Assert.Equal("SID999", received.Segment("OBR")!.Field(2));

        var parsedAck = Hl7Parser.Parse(ack);
        Assert.Equal("ACK", parsedAck.MessageType);
        Assert.Equal("AA", parsedAck.Segment("MSA")!.Field(1));
        Assert.Equal("CTRL-42", parsedAck.Segment("MSA")!.Field(2));

        await listener.StopAsync();
    }

    [Fact]
    public async Task Listener_UsesCustomAcknowledgementFromHandler()
    {
        using var listener = new MllpListener(port: 0);
        listener.MessageReceived += (_, e) =>
            e.Acknowledgement = "MSH|^~\\&|SimuLink|Roche|LIS|H|20260624120000||ACK|X|P|2.5.1\rMSA|AR|CTRL-42";
        listener.Start();

        using var client = new MllpClient("127.0.0.1", listener.Port);
        var ack = await client.SendAsync(Order);

        var parsedAck = Hl7Parser.Parse(ack);
        Assert.Equal("AR", parsedAck.Segment("MSA")!.Field(1));

        await listener.StopAsync();
    }

    [Fact]
    public async Task Client_CanSendMultipleMessagesOnOneConnection()
    {
        using var listener = new MllpListener(port: 0);
        var count = 0;
        listener.MessageReceived += (_, _) => Interlocked.Increment(ref count);
        listener.Start();

        using var client = new MllpClient("127.0.0.1", listener.Port);
        await client.ConnectAsync();
        await client.SendAsync(Order);
        await client.SendAsync(Order);

        Assert.Equal(2, count);

        await listener.StopAsync();
    }
}
