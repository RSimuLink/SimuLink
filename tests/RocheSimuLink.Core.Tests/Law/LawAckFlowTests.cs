using RocheSimuLink.HL7.Law;
using RocheSimuLink.HL7.Parsers;
using RocheSimuLink.Models.Law;
using Xunit;

namespace RocheSimuLink.Core.Tests.Law;

/// <summary>
/// Verifies the result acknowledgment flow (ACK^R22) per the x800 Data Manager
/// Host Interface Manual: positive acks carry MSH + MSA only; negative acks add
/// ERR segments.
/// </summary>
public class LawAckFlowTests
{
    private static string[] Segments(string raw) => raw.Split('\r');

    private static LawMessageHeader AckHeader() => new()
    {
        SendingApplication = "HOST",
        ReceivingApplication = "X800DM",
        MessageDateTime = "20241030110040+0100",
        MessageControlId = "a1b2c3d4-0000-1111-2222-333344445555",
    };

    [Fact]
    public void Ack_Positive_IsMshMsaOnly()
    {
        var msg = LawAckR22Builder.Build(new ResultAcknowledgment
        {
            Header = AckHeader(),
            AcknowledgmentCode = "AA",
            AcknowledgedControlId = "ffc2e7b1-e2a1-467d-aba8-831b3c0c2272",
        });
        var segs = Segments(msg.RawMessage);

        Assert.Equal("ACK^R22", msg.MessageType);
        Assert.Equal(2, segs.Length);
        Assert.Equal(
            "MSH|^~\\&|HOST||X800DM||20241030110040+0100||ACK^R22^ACK|" +
            "a1b2c3d4-0000-1111-2222-333344445555|P|2.5.1||||||UNICODE UTF-8|||LAB-29^IHE",
            segs[0]);
        Assert.Equal("MSA|AA|ffc2e7b1-e2a1-467d-aba8-831b3c0c2272", segs[1]);
    }

    [Fact]
    public void Ack_Positive_DropsErrorsEvenIfSupplied()
    {
        var msg = LawAckR22Builder.Build(new ResultAcknowledgment
        {
            Header = AckHeader(),
            AcknowledgmentCode = "AA",
            AcknowledgedControlId = "x",
            Errors =
            {
                new AcknowledgmentError { SegmentId = "OBX", SegmentSequence = "1" },
            },
        });

        Assert.DoesNotContain("ERR|", msg.RawMessage);
    }

    [Fact]
    public void Ack_Negative_IncludesErrSegment()
    {
        var msg = LawAckR22Builder.Build(new ResultAcknowledgment
        {
            Header = AckHeader(),
            AcknowledgmentCode = "AE",
            AcknowledgedControlId = "ffc2e7b1-e2a1-467d-aba8-831b3c0c2272",
            Errors =
            {
                new AcknowledgmentError
                {
                    SegmentId = "OBX",
                    SegmentSequence = "1",
                    FieldNumber = "2",
                    ErrorCode = new CodedElement("207", "", "HL70357"),
                    Severity = "E",
                    UserMessage = "Unknown value type",
                },
            },
        });
        var err = Segments(msg.RawMessage).First(s => s.StartsWith("ERR"));

        Assert.Equal("ERR||OBX^1^2|207^^HL70357|E||||Unknown value type", err);
    }

    [Fact]
    public void Ack_Positive_RoundTripsThroughParser()
    {
        var msg = LawAckR22Builder.Build(new ResultAcknowledgment
        {
            Header = AckHeader(),
            AcknowledgmentCode = "AA",
            AcknowledgedControlId = "ffc2e7b1-e2a1-467d-aba8-831b3c0c2272",
        });
        var parsed = LawAckParser.Parse(msg.RawMessage);

        Assert.Equal("AA", parsed.AcknowledgmentCode);
        Assert.Equal("ffc2e7b1-e2a1-467d-aba8-831b3c0c2272", parsed.AcknowledgedControlId);
        Assert.Empty(parsed.Errors);
        Assert.Equal("ACK^R22", parsed.Header.MessageTypeCode);
    }

    [Fact]
    public void Ack_Negative_RoundTripsThroughParser()
    {
        var msg = LawAckR22Builder.Build(new ResultAcknowledgment
        {
            Header = AckHeader(),
            AcknowledgmentCode = "AE",
            AcknowledgedControlId = "x",
            Errors =
            {
                new AcknowledgmentError
                {
                    SegmentId = "OBX",
                    SegmentSequence = "1",
                    FieldNumber = "2",
                    ErrorCode = new CodedElement("207", "", "HL70357"),
                    Severity = "E",
                    UserMessage = "Unknown value type",
                },
            },
        });
        var parsed = LawAckParser.Parse(msg.RawMessage);

        Assert.Equal("AE", parsed.AcknowledgmentCode);
        var error = Assert.Single(parsed.Errors);
        Assert.Equal("OBX", error.SegmentId);
        Assert.Equal("1", error.SegmentSequence);
        Assert.Equal("2", error.FieldNumber);
        Assert.Equal("207", error.ErrorCode.Identifier);
        Assert.Equal("E", error.Severity);
        Assert.Equal("Unknown value type", error.UserMessage);
    }
}
