using RocheLIT.HL7.Law;
using RocheLIT.HL7.Parsers;
using RocheLIT.Models;
using Xunit;

namespace RocheLIT.Core.Tests.Law;

/// <summary>
/// Verifies the projection from the UI's flat result selection into the rich
/// <see cref="RocheLIT.Models.Law.LawResultMessage"/> used by
/// <see cref="LawOulR22Builder"/>.
/// </summary>
public class LawResultMessageFactoryTests
{
    private static readonly DateTimeOffset When =
        new(2024, 10, 29, 17, 29, 20, TimeSpan.FromHours(1));

    private static SampleType Plasma => new() { DisplayName = "Plasma", Hl7Code = "PLAS" };

    private static ConnectionSettings Settings => new()
    {
        SendingApplication = "X800DM",
        ReceivingApplication = "Host",
    };

    private static TestType MultiTargetTest() => new()
    {
        Name = "CT/NG",
        UniversalServiceIdentifier = "72828-7^CT/NG^LN",
        Targets =
        {
            new Target
            {
                Name = "CT",
                ObservationIdentifier = "CT^Chlamydia^99ROC",
                ObservationValues = { "Negative" },
            },
            new Target
            {
                Name = "NG",
                ObservationIdentifier = "NG^Gonorrhoeae^99ROC",
                ObservationValues = { "Negative" },
            },
        },
    };

    private static TestType WnvTest() => new()
    {
        Name = "WNV",
        UniversalServiceIdentifier = "74857-4^WNV^LN",
        Targets =
        {
            new Target
            {
                Name = "WNV",
                ObservationIdentifier = "WNV^WNV^99ROC",
                ObservationValues = { "RR", "NR" },
                InterpretationCodes = { "Reactive", "Non-Reactive" },
            },
        },
    };

    [Fact]
    public void Create_MapsSpecimenAndTestCode()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, Settings, When);

        Assert.Equal("SID42", msg.Specimen.SampleId);
        Assert.Equal("PLAS", msg.Specimen.SpecimenType.Identifier);
        Assert.Equal("X800DM", msg.SendingApplication);
        Assert.Equal("Host", msg.ReceivingApplication);
        var testResult = Assert.Single(msg.Tests);
        Assert.Equal("72828-7", testResult.TestCode.Identifier);
        Assert.Equal("CT/NG", testResult.TestCode.Text);
        Assert.Equal("LN", testResult.TestCode.CodingSystem);
    }

    [Fact]
    public void Create_EmitsOneObxPerTarget()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, Settings, When);

        var observations = msg.Tests[0].Observations;
        Assert.Equal(2, observations.Count);
        Assert.Equal("1", observations[0].SetId);
        Assert.Equal("2", observations[1].SetId);
        Assert.Equal("CT", observations[0].ObservationId.Identifier);
        Assert.Equal("NG", observations[1].ObservationId.Identifier);
    }

    [Fact]
    public void Create_SelectedTargetCarriesValueAndFlag_OthersFallBack()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, Settings, When);

        var selected = msg.Tests[0].Observations[0];
        var other = msg.Tests[0].Observations[1];

        Assert.Equal("Positive", selected.Value);
        Assert.Equal("H", selected.Interpretation!.Identifier);

        // Non-selected target falls back to its first configured value, normal flag.
        Assert.Equal("Negative", other.Value);
        Assert.Equal("N", other.Interpretation!.Identifier);
    }

    [Fact]
    public void Create_AppliesFinalStatusAndTimestampToAllChannels()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[1], "Positive",
            ResultFlag.Critical, Settings, When);

        Assert.All(msg.Tests[0].Observations, o =>
        {
            Assert.Equal("F", o.Status);
            Assert.Equal("20241029172920", o.AnalysisDateTime);
            Assert.Equal("RSLT", o.ObservationType);
        });

        // Selected target is the second one here.
        Assert.Equal("AA", msg.Tests[0].Observations[1].Interpretation!.Identifier);
    }

    [Fact]
    public void Create_MessageDateTimeUsesHl7OffsetFormat()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, Settings, When);

        Assert.Equal("20241029172920+0100", msg.MessageDateTime);
    }

    [Fact]
    public void Create_MapsManualResultCodeToObxValueAndInterpretation()
    {
        var test = WnvTest();
        var msg = LawResultMessageFactory.Create(
            "$ABC123456",
            new SampleType
            {
                DisplayName = "Cadaveric Plasma",
                Hl7Code = "CP",
                SpecimenCode = "CP^cadavericPlasma^99ROC",
            },
            test,
            test.Targets[0],
            "RR",
            ResultFlag.Normal,
            Settings,
            When,
            "150 uL",
            rackId: "RACK9",
            carrierPosition: "12");

        var observation = Assert.Single(msg.Tests[0].Observations);
        Assert.Equal("RACK9", msg.Specimen.CarrierId);
        Assert.Equal("12", msg.Specimen.CarrierPosition);
        Assert.Equal("Reactive", observation.Value);
        Assert.Equal("RR", observation.Interpretation!.Identifier);
        Assert.Equal("99ROC", observation.Interpretation.CodingSystem);
        Assert.Equal("X800DMSYSTEM", observation.ResponsibleObserver);
        Assert.Equal("c6800^Roche~c6800.504^Roche", observation.ObservationMethod);
        Assert.Equal("150^uL&&UCUM", msg.Tests[0].ConsumptionVolume);
    }

    [Fact]
    public void Create_ProducesBuildableOulR22()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, Settings, When);

        var built = LawOulR22Builder.Build(msg);
        var parsed = Hl7Parser.Parse(built.RawMessage);

        Assert.Equal("OUL^R22", parsed.MessageType);
        Assert.Equal(2, parsed.AllSegments("OBX").Count());
        Assert.NotNull(parsed.Segment("SPM"));
        Assert.NotNull(parsed.Segment("SAC"));
    }

    [Fact]
    public void Create_IncludesInventoryAndCtValuesInBuildableOulR22WhenRequested()
    {
        var test = WnvTest();
        var msg = LawResultMessageFactory.Create(
            "$ABC123456",
            new SampleType
            {
                DisplayName = "Cadaveric Plasma",
                Hl7Code = "CP",
                SpecimenCode = "CP^cadavericPlasma^99ROC",
            },
            test,
            test.Targets[0],
            "RR",
            ResultFlag.Normal,
            Settings,
            When,
            "150 uL",
            rackId: "1897",
            carrierPosition: "5",
            includeInventory: true,
            includeCtValues: true);

        var segments = LawOulR22Builder.Build(msg).RawMessage.Split('\r');
        var names = segments.Select(s => s[..3]).ToArray();

        Assert.Equal(
            new[] { "MSH", "SPM", "SAC", "OBR", "ORC", "OBX", "TCD", "INV", "INV", "INV", "INV", "INV", "INV", "INV", "OBX" },
            names);
        Assert.Equal(7, segments.Count(s => s.StartsWith("INV")));
        Assert.Contains(
            "INV|Wash reagent^^99ROC|OK^^HL70383|LI^^HL70384|||||||||20260228225959+0100||||M03540",
            segments);

        var ctValues = Assert.Single(segments, s => s.StartsWith("OBX") && s.Contains("S_OTHER"));
        Assert.StartsWith(
            "OBX|2|NA|WNV^WNV^99ROC^S_OTHER^Other Supplemental^IHELAW|1|37.04^36.32",
            ctValues);
    }
}
