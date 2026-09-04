using RocheLIT.HL7.Law;
using RocheLIT.Models;
using RocheLIT.Models.Ucap;
using Xunit;

namespace RocheLIT.Core.Tests.Law;

public class LawUcapResultMessageFactoryTests
{
    private static readonly DateTimeOffset When =
        new(2026, 9, 1, 10, 28, 29, TimeSpan.FromHours(2));

    private static ConnectionSettings Settings => new()
    {
        SendingApplication = "LIT",
        ReceivingApplication = "LIS",
    };

    [Fact]
    public void Create_BuildsUcapOulR22WithCustomUsidAndTargets()
    {
        var request = new UcapResultRequest
        {
            SampleId = "$UCAP12345",
            TestNameSuffix = "ABCD1234",
            UniversalServiceId = "0000041",
            SampleType = UcapCatalog.SampleTypes().Single(s => s.Hl7Code == "PLAS"),
            SampleVolume = "350 uL",
            RackId = "RACK12",
            CarrierPosition = "7",
            Targets =
            {
                new UcapTargetResult { TargetName = "Target1", ResultValue = "Reactive" },
                new UcapTargetResult { TargetName = "Target2", ResultValue = "Non-Reactive" },
            },
        };

        var message = LawUcapResultMessageFactory.Create(request, Settings, When);
        message.MessageControlId = "11111111-2222-3333-4444-555555555555";

        var segments = LawOulR22Builder.Build(message).RawMessage.Split('\r');

        Assert.Equal("SPM|1|$UCAP12345&ROCHE||PLAS^plasma^HL70487|||||||P^^HL70369", segments[1]);
        Assert.Equal("SAC|||$UCAP12345|||||||RACK12|7", segments[2]);
        Assert.Equal("OBR||||0000041^UCAP^99ROC", segments[3]);
        Assert.Equal("ORC|SC||||CM", segments[4]);
        Assert.Equal(
            "OBX|1|ST|Target1^Target1^99ROC|1|Reactive|||RR^^99ROC|||F|||||LITSYSTEM||c6800^Roche~c6800.504^Roche|20260901102829||||||||||RSLT",
            segments[5]);
        Assert.Equal("TCD|0000041^UCAP^99ROC||||||||350^uL&&UCUM", segments[6]);
        Assert.Equal(
            "OBX|2|ST|Target2^Target2^99ROC|1|Non-Reactive|||NR^^99ROC|||F|||||LITSYSTEM||c6800^Roche~c6800.504^Roche|20260901102829||||||||||RSLT",
            segments[7]);
    }

    [Fact]
    public void Create_ValidatesUcapRequiredFormats()
    {
        var request = new UcapResultRequest
        {
            SampleId = "$UCAP12345",
            TestNameSuffix = "ABCD1234",
            UniversalServiceId = "123456",
            SampleType = UcapCatalog.SampleTypes().First(),
            SampleVolume = "400 uL",
            Targets =
            {
                new UcapTargetResult { TargetName = "Target1", ResultValue = "Reactive" },
            },
        };

        var ex = Assert.Throws<ArgumentException>(() =>
            LawUcapResultMessageFactory.Create(request, Settings, When));
        Assert.Contains("USID", ex.Message);
    }

    [Fact]
    public void UcapCatalog_IncludesManualSampleTypesAndVolumes()
    {
        var samples = UcapCatalog.SampleTypes();

        var plasma = Assert.Single(samples, s => s.Hl7Code == "PLAS");
        Assert.Equal(new[] { "200 uL", "350 uL", "500 uL", "800 uL" },
            plasma.AllowedVolumes.Select(v => v.Volume));

        var simple = Assert.Single(samples, s => s.Hl7Code == "UCSimpS");
        Assert.Equal("U_Simple sample", simple.DisplayName);
        Assert.Equal("UCSimpS^UC_Simple sample^99ROC", simple.SpecimenCode);
    }
}
