using RocheSimuLink.HL7.Law;
using RocheSimuLink.HL7.Parsers;
using RocheSimuLink.Models.Law;
using Xunit;

namespace RocheSimuLink.Core.Tests.Law;

/// <summary>
/// Verifies the work-order query flow (QBP^Q11 / RSP^K11) against the example
/// messages in the x800 Data Manager Host Interface Manual.
/// </summary>
public class LawQueryFlowTests
{
    private static string[] Segments(string raw) => raw.Split('\r');

    private static WorkOrderQuery BuildQuery() => new()
    {
        Header = new LawMessageHeader
        {
            SendingApplication = "X800 DM",
            ReceivingApplication = "HOST",
            MessageDateTime = "20210915103321+0200",
            MessageControlId = "e6c753d2-c752-4de4-b86d-0a9507000ce0",
        },
        QueryTag = "75566b64-33be-4a3d-a154-b895305b271c",
        SampleId = "sample123",
        CarrierId = "S54",
        CarrierPosition = "14",
        QueryPriority = "I",
    };

    [Fact]
    public void Qbp_MatchesTrace()
    {
        var msg = LawQbpQ11Builder.Build(BuildQuery());
        var segs = Segments(msg.RawMessage);

        Assert.Equal(
            "MSH|^~\\&|X800 DM||HOST||20210915103321+0200||QBP^Q11^QBP_Q11|" +
            "e6c753d2-c752-4de4-b86d-0a9507000ce0|P|2.5.1|||NE|AL||UNICODE UTF-8|||LAB-27R^ROCHE",
            segs[0]);
        Assert.Equal(
            "QPD|WOS_ROCHE^Work Order Step Roche Extension^ROCHE|" +
            "75566b64-33be-4a3d-a154-b895305b271c|sample123|S54|14",
            segs[1]);
        Assert.Equal("RCP|I||R^Real Time^HL70394", segs[2]);
    }

    [Fact]
    public void Qbp_RoundTripsThroughParser()
    {
        var msg = LawQbpQ11Builder.Build(BuildQuery());
        var parsed = LawQueryParser.ParseQuery(msg.RawMessage);

        Assert.Equal("sample123", parsed.SampleId);
        Assert.Equal("S54", parsed.CarrierId);
        Assert.Equal("14", parsed.CarrierPosition);
        Assert.Equal("75566b64-33be-4a3d-a154-b895305b271c", parsed.QueryTag);
        Assert.Equal("I", parsed.QueryPriority);
        Assert.Equal("R", parsed.ResponseModality.Identifier);
        Assert.Equal("QBP^Q11", parsed.Header.MessageTypeCode);
    }

    private static WorkOrderQueryResponse BuildResponse() => new()
    {
        Header = new LawMessageHeader
        {
            SendingApplication = "HOST",
            ReceivingApplication = "X800 DM",
            MessageDateTime = "20210915123329+0200",
            MessageControlId = "39a277e3-3b3a-43a7-a7ba-01b01e130216",
        },
        AcknowledgmentCode = "AA",
        AcknowledgedControlId = "e6c753d2-c752-4de4-b86d-0a9507000ce0",
        QueryTag = "75566b64-33be-4a3d-a154-b895305b271c",
        QueryResponseStatus = "OK",
        EchoedQueryTag = "75566b64-33be-4a3d-a154-b895305b271c",
        SampleId = "sample123",
        CarrierId = "S54",
        CarrierPosition = "14",
    };

    [Fact]
    public void Rsp_MatchesTrace()
    {
        var msg = LawRspK11Builder.Build(BuildResponse());
        var segs = Segments(msg.RawMessage);

        Assert.Equal(
            "MSH|^~\\&|HOST||X800 DM||20210915123329+0200||RSP^K11^RSP_K11|" +
            "39a277e3-3b3a-43a7-a7ba-01b01e130216|P|2.5.1||||||UNICODE UTF-8|||LAB-27R^ROCHE",
            segs[0]);
        Assert.Equal("MSA|AA|e6c753d2-c752-4de4-b86d-0a9507000ce0", segs[1]);
        Assert.Equal(
            "QAK|75566b64-33be-4a3d-a154-b895305b271c|OK|" +
            "WOS_ROCHE^Work Order Step Roche Extension^ROCHE",
            segs[2]);
        Assert.Equal(
            "QPD|WOS_ROCHE^Work Order Step Roche Extension^ROCHE|" +
            "75566b64-33be-4a3d-a154-b895305b271c|sample123|S54|14",
            segs[3]);
    }

    [Fact]
    public void Rsp_RoundTripsThroughParser()
    {
        var msg = LawRspK11Builder.Build(BuildResponse());
        var parsed = LawQueryParser.ParseResponse(msg.RawMessage);

        Assert.Equal("AA", parsed.AcknowledgmentCode);
        Assert.Equal("e6c753d2-c752-4de4-b86d-0a9507000ce0", parsed.AcknowledgedControlId);
        Assert.Equal("75566b64-33be-4a3d-a154-b895305b271c", parsed.QueryTag);
        Assert.Equal("OK", parsed.QueryResponseStatus);
        Assert.Equal("sample123", parsed.SampleId);
        Assert.Equal("RSP^K11", parsed.Header.MessageTypeCode);
    }
}
