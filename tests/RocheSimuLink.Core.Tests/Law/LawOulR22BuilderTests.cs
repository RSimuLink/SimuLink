using RocheSimuLink.HL7.Law;
using RocheSimuLink.Models.Law;
using Xunit;

namespace RocheSimuLink.Core.Tests.Law;

public class LawOulR22BuilderTests
{
    /// <summary>
    /// Builds the HIV-1 OUL^R22 from the model and verifies key segments match
    /// the authoritative trace in "HIV HL7 Example.pdf".
    /// </summary>
    private static LawResultMessage BuildHivMessage() => new()
    {
        SendingApplication = "X800DM",
        ReceivingApplication = "Host",
        MessageDateTime = "20241030110039+0100",
        MessageControlId = "ffc2e7b1-e2a1-467d-aba8-831b3c0c2272",
        Specimen = new Specimen
        {
            SampleId = "$0E0EYXDR",
            Namespace = "ROCHE",
            SpecimenType = new CodedElement("PLAS", "plasma", "HL70487"),
            Role = "P",
            CarrierId = "1692",
            CarrierPosition = "2",
        },
        Tests =
        {
            new LawTestResult
            {
                SetId = "1",
                TestCode = new CodedElement("70241-5", "HIV", "LN"),
                OrderControl = "SC",
                OrderStatus = "CM",
                ConsumptionVolume = "500^uL&&UCUM",
                Observations =
                {
                    new ChannelResult
                    {
                        ValueType = "NM",
                        ObservationId = new CodedElement("HIV", "HIV", "99ROC"),
                        SubId = "1",
                        Value = "130",
                        Units = new CodedElement("10*2.{copies}/mL", "", "UCUM"),
                        Interpretation = new CodedElement("VAL", "", "99ROC"),
                        ResultStatus = "F",
                        ObservationMethod = "c6800^Roche~c6800.504^Roche",
                        AnalysisDateTime = "20241029172920",
                        EquipmentInstanceId = "6-504-241029-0092",
                        ObservationType = "RSLT",
                    },
                },
                Reagents =
                {
                    new ReagentInventory
                    {
                        SubstanceId = new CodedElement("Reagent cassette", "", "99ROC"),
                        Status = new CodedElement("OK", "", "HL70383"),
                        SubstanceType = new CodedElement("MR", "", "HL70384"),
                        ExpiryDateTime = "20291231225959+0100",
                        LotNumber = "HHHHIV",
                    },
                },
            },
        },
    };

    private static string[] Segments(string raw) => raw.Split('\r');

    [Fact]
    public void Build_MshMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var msh = Segments(msg.RawMessage)[0];

        Assert.Equal(
            "MSH|^~\\&|X800DM||Host||20241030110039+0100||OUL^R22^OUL_R22|" +
            "ffc2e7b1-e2a1-467d-aba8-831b3c0c2272|P|2.5.1|||NE|AL||UNICODE UTF-8|||LAB-29^IHE",
            msh);
    }

    [Fact]
    public void Build_SpmMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var spm = Segments(msg.RawMessage).First(s => s.StartsWith("SPM"));

        Assert.Equal("SPM|1|$0E0EYXDR&ROCHE||PLAS^plasma^HL70487|||||||P^^HL70369", spm);
    }

    [Fact]
    public void Build_SacMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var sac = Segments(msg.RawMessage).First(s => s.StartsWith("SAC"));

        Assert.Equal("SAC|||$0E0EYXDR|||||||1692|2", sac);
    }

    [Fact]
    public void Build_ObrMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var obr = Segments(msg.RawMessage).First(s => s.StartsWith("OBR"));

        Assert.Equal("OBR||1||70241-5^HIV^LN", obr);
    }

    [Fact]
    public void Build_OrcMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var orc = Segments(msg.RawMessage).First(s => s.StartsWith("ORC"));

        Assert.Equal("ORC|SC|1|||CM", orc);
    }

    [Fact]
    public void Build_TcdMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var tcd = Segments(msg.RawMessage).First(s => s.StartsWith("TCD"));

        Assert.Equal("TCD|70241-5^HIV^LN||||||||500^uL&&UCUM", tcd);
    }

    [Fact]
    public void Build_ObxMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var obx = Segments(msg.RawMessage).First(s => s.StartsWith("OBX"));

        Assert.Equal(
            "OBX|1|NM|HIV^HIV^99ROC|1|130|10*2.{copies}/mL^^UCUM||VAL^^99ROC|||F|||||" +
            "c6800^Roche~c6800.504^Roche||20241029172920||6-504-241029-0092|||||||||RSLT",
            obx);
    }

    [Fact]
    public void Build_InvMatchesTrace()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var inv = Segments(msg.RawMessage).First(s => s.StartsWith("INV"));

        Assert.Equal(
            "INV|Reagent cassette^^99ROC|OK^^HL70383|MR^^HL70384|||||||||20291231225959+0100||||HHHHIV",
            inv);
    }

    [Fact]
    public void Build_SegmentOrderIsMshSpmSacObrOrcTcdObxInv()
    {
        var msg = LawOulR22Builder.Build(BuildHivMessage());
        var names = Segments(msg.RawMessage).Select(s => s[..3]).ToArray();

        Assert.Equal(
            new[] { "MSH", "SPM", "SAC", "OBR", "ORC", "TCD", "OBX", "INV" },
            names);
    }
}
