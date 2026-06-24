using RocheSimuLink.Services;
using Xunit;

namespace RocheSimuLink.Core.Tests.Services;

public class SettingsLoaderTests
{
    [Fact]
    public void Load_ReturnsNonEmptyCollections()
    {
        var settings = SettingsLoader.Load();

        Assert.NotEmpty(settings.TestTypes);
        Assert.NotEmpty(settings.SampleTypes);
        Assert.NotEmpty(settings.SampleVolumes);
        Assert.NotEmpty(settings.Workflows);
    }

    [Fact]
    public void Load_EveryTestHasAtLeastOneTarget()
    {
        var settings = SettingsLoader.Load();

        Assert.All(settings.TestTypes, t => Assert.NotEmpty(t.Targets));
    }

    [Fact]
    public void Load_EveryTargetHasMatchingValueAndInterpretationCounts()
    {
        var settings = SettingsLoader.Load();

        // Values and interpretation codes are always paired one-to-one.
        foreach (var test in settings.TestTypes)
        {
            foreach (var target in test.Targets)
            {
                Assert.Equal(target.ObservationValues.Count, target.InterpretationCodes.Count);
            }
        }
    }

    [Fact]
    public void Load_TargetsCarryResultValuesFromTheManual()
    {
        var settings = SettingsLoader.Load();

        // The bundled HIM yields OBX-5 result codes for most targets, so the
        // result dropdown is populated out of the box rather than empty.
        var targets = settings.TestTypes.SelectMany(t => t.Targets).ToList();
        Assert.NotEmpty(targets);
        Assert.Contains(targets, t => t.ObservationValues.Count > 0);
    }

    [Fact]
    public void Load_EveryTestHasUniversalServiceIdentifier()
    {
        var settings = SettingsLoader.Load();

        Assert.All(settings.TestTypes,
            t => Assert.False(string.IsNullOrWhiteSpace(t.UniversalServiceIdentifier)));
    }

    [Fact]
    public void Load_CatalogIsSourcedFromBundledManual()
    {
        var settings = SettingsLoader.Load();

        // Landmarks that only exist in the bundled HIM, proving the default
        // catalog is parsed from the manual rather than hand-seeded.
        Assert.Contains(settings.TestTypes,
            t => t.UniversalServiceIdentifier == "72828-7^CT/NG^LN");
        Assert.Contains(settings.SampleTypes, s => s.Hl7Code == "PLAS");

        // LOINC-coded test identifiers are a HIM-only trait (the old seed used
        // local "^L" codes).
        Assert.Contains(settings.TestTypes,
            t => t.UniversalServiceIdentifier.EndsWith("^LN", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_SampleTypesHaveHl7Codes()
    {
        var settings = SettingsLoader.Load();

        Assert.All(settings.SampleTypes, s => Assert.False(string.IsNullOrWhiteSpace(s.Hl7Code)));
    }

    [Fact]
    public void Load_WorkflowsCoverAllSupportedKinds()
    {
        var settings = SettingsLoader.Load();
        var kinds = settings.Workflows.Select(w => w.Kind).Distinct().ToList();

        var allKinds = Enum.GetValues<RocheSimuLink.Models.Workflows.SupportedWorkflows>();
        Assert.Equal(allKinds.Length, kinds.Count);
    }
}
