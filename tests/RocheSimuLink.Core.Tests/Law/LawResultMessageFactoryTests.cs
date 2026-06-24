using RocheSimuLink.HL7.Law;
using RocheSimuLink.HL7.Parsers;
using RocheSimuLink.Models;
using Xunit;

namespace RocheSimuLink.Core.Tests.Law;

/// <summary>
/// Verifies the projection from the UI's flat result selection into the rich
/// <see cref="RocheSimuLink.Models.Law.LawResultMessage"/> used by
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

    [Fact]
    public void Create_MapsSpecimenAndTestCode()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, ResultStatus.Final, Settings, When);

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
            ResultFlag.High, ResultStatus.Final, Settings, When);

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
            ResultFlag.High, ResultStatus.Final, Settings, When);

        var selected = msg.Tests[0].Observations[0];
        var other = msg.Tests[0].Observations[1];

        Assert.Equal("Positive", selected.Value);
        Assert.Equal("H", selected.Interpretation!.Identifier);

        // Non-selected target falls back to its first configured value, normal flag.
        Assert.Equal("Negative", other.Value);
        Assert.Equal("N", other.Interpretation!.Identifier);
    }

    [Fact]
    public void Create_AppliesStatusAndTimestampToAllChannels()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[1], "Positive",
            ResultFlag.Critical, ResultStatus.Preliminary, Settings, When);

        Assert.All(msg.Tests[0].Observations, o =>
        {
            Assert.Equal("P", o.ResultStatus);
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
            ResultFlag.High, ResultStatus.Final, Settings, When);

        Assert.Equal("20241029172920+0100", msg.MessageDateTime);
    }

    [Fact]
    public void Create_ProducesBuildableOulR22()
    {
        var test = MultiTargetTest();
        var msg = LawResultMessageFactory.Create(
            "SID42", Plasma, test, test.Targets[0], "Positive",
            ResultFlag.High, ResultStatus.Final, Settings, When);

        var built = LawOulR22Builder.Build(msg);
        var parsed = Hl7Parser.Parse(built.RawMessage);

        Assert.Equal("OUL^R22", parsed.MessageType);
        Assert.Equal(2, parsed.AllSegments("OBX").Count());
        Assert.NotNull(parsed.Segment("SPM"));
        Assert.NotNull(parsed.Segment("SAC"));
    }
}
