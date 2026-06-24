using RocheSimuLink.HL7.Workflows;
using RocheSimuLink.Models;
using Xunit;

namespace RocheSimuLink.Core.Tests.Workflows;

public class WorkflowBuilderTests
{
    private static SampleType Serum => new() { DisplayName = "Serum", Hl7Code = "SER" };

    private static TestType GlucoseTest => new()
    {
        Name = "Glucose",
        UniversalServiceIdentifier = "GLU^Glucose^L",
        Targets =
        {
            new Target
            {
                Name = "Glucose",
                ObservationIdentifier = "GLU^Glucose^L",
                ObservationValues = { "140" },
                InterpretationCodes = { "H" },
            },
        },
    };

    private static string[] Segments(string raw) => raw.Split('\r');

    [Fact]
    public void OulR22_HasExpectedSegmentOrder()
    {
        var msg = OulR22Builder.Build("SID1", Serum, GlucoseTest, GlucoseTest.Targets[0], "140", "H");
        var segs = Segments(msg.RawMessage);

        Assert.Equal("OUL^R22", msg.MessageType);
        Assert.Equal(5, segs.Length);
        Assert.StartsWith("MSH|", segs[0]);
        Assert.StartsWith("PID|", segs[1]);
        Assert.StartsWith("SPM|", segs[2]);
        Assert.StartsWith("OBR|", segs[3]);
        Assert.StartsWith("OBX|", segs[4]);
    }

    [Fact]
    public void OulR22_CarriesObservationValueIntoObx()
    {
        var msg = OulR22Builder.Build("SID1", Serum, GlucoseTest, GlucoseTest.Targets[0], "140", "H");

        Assert.Contains("OBX|1|ST|GLU^Glucose^L||140|||H", msg.RawMessage);
    }

    [Fact]
    public void OmlO33_BuildsOrderWithoutResult()
    {
        var msg = OmlO33Builder.Build("SID1", Serum, GlucoseTest);
        var segs = Segments(msg.RawMessage);

        Assert.Equal("OML^O33", msg.MessageType);
        Assert.Equal(4, segs.Length);
        Assert.DoesNotContain("OBX|", msg.RawMessage);
    }

    [Fact]
    public void OrlO34_IncludesOrcSegment()
    {
        var msg = OrlO34Builder.Build("SID1", Serum, GlucoseTest);

        Assert.Equal("ORL^O34", msg.MessageType);
        Assert.Contains("ORC|", msg.RawMessage);
    }

    [Fact]
    public void RspK11_IncludesQueryAcknowledgement()
    {
        var msg = RspK11Builder.Build("SID1", Serum, GlucoseTest);

        Assert.Equal("RSP^K11", msg.MessageType);
        Assert.Contains("MSA|AA|SID1", msg.RawMessage);
        Assert.Contains("QAK|OK", msg.RawMessage);
    }

    [Fact]
    public void QbpQ11_RequestsImmediateResponse()
    {
        var msg = QbpQ11Builder.Build("INST-1");

        Assert.Equal("QBP^Q11", msg.MessageType);
        Assert.Contains("RCP|I", msg.RawMessage);
    }

    [Fact]
    public void AckR22_AcceptsMessage()
    {
        var msg = AckR22Builder.Build("SID1");

        Assert.Equal("ACK^R22", msg.MessageType);
        Assert.Contains("MSA|AA|SID1", msg.RawMessage);
    }

    [Fact]
    public void OulR22_WithSettings_UsesConfiguredMshIdentities()
    {
        var settings = new RocheSimuLink.Models.ConnectionSettings
        {
            SendingApplication = "MYAPP",
            SendingFacility = "MYFAC",
            ReceivingApplication = "THELIS",
            ReceivingFacility = "THEHOSP",
            Hl7Version = "2.8",
        };

        var msg = OulR22Builder.Build(
            "SID1", Serum, GlucoseTest, GlucoseTest.Targets[0], "140",
            RocheSimuLink.Models.ResultFlag.High, RocheSimuLink.Models.ResultStatus.Final, settings);

        var mshFields = msg.RawMessage.Split('\r')[0].Split('|');
        Assert.Equal("MYAPP", mshFields[2]);
        Assert.Equal("MYFAC", mshFields[3]);
        Assert.Equal("THELIS", mshFields[4]);
        Assert.Equal("THEHOSP", mshFields[5]);
        Assert.Equal("2.8", mshFields[11]);
    }

    [Fact]
    public void OulR22_WithFlagAndStatus_PopulatesObx()
    {
        var msg = OulR22Builder.Build(
            "SID1", Serum, GlucoseTest, GlucoseTest.Targets[0], "Positive",
            RocheSimuLink.Models.ResultFlag.Critical, RocheSimuLink.Models.ResultStatus.Preliminary);

        var obx = msg.RawMessage.Split('\r').First(s => s.StartsWith("OBX")).Split('|');
        Assert.Equal("Positive", obx[5]);
        Assert.Equal("AA", obx[8]);  // Critical
        Assert.Equal("P", obx[11]);  // Preliminary
    }

    [Fact]
    public void AllMessages_UseCarriageReturnSegmentSeparator()
    {
        var msg = OulR22Builder.Build("SID1", Serum, GlucoseTest, GlucoseTest.Targets[0], "140", "H");

        Assert.Contains('\r', msg.RawMessage);
        Assert.DoesNotContain('\n', msg.RawMessage);
    }
}
