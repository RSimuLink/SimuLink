using RocheLIT.Models;
using RocheLIT.Services;
using Xunit;

namespace RocheLIT.Core.Tests.Services;

/// <summary>
/// Unit tests for the result-entry presentation logic extracted from MainForm.
/// </summary>
public class ResultEntryPresenterTests
{
    private static TestType TestWith(params string[] leadValues) => new()
    {
        Name = "CT/NG",
        Targets =
        {
            new Target
            {
                Name = "CT",
                ObservationValues = leadValues.ToList(),
            },
        },
    };

    // --- ResultValuesFor ----------------------------------------------------

    [Fact]
    public void ResultValuesFor_ReturnsLeadTargetValues()
    {
        var test = TestWith("POS", "NEG");

        Assert.Equal(new[] { "POS", "NEG" }, ResultEntryPresenter.ResultValuesFor(test));
    }

    [Fact]
    public void ResultValuesFor_NullTest_IsEmpty()
    {
        Assert.Empty(ResultEntryPresenter.ResultValuesFor(null));
    }

    [Fact]
    public void ResultValuesFor_TestWithoutTargets_IsEmpty()
    {
        Assert.Empty(ResultEntryPresenter.ResultValuesFor(new TestType()));
    }

    // --- VolumesFor ---------------------------------------------------------

    [Fact]
    public void VolumesFor_TestWithAllowedVolumes_UsesThem()
    {
        var test = new TestType { AllowedVolumes = { new SampleVolume { Volume = "850 uL" } } };
        var catalog = new List<SampleVolume> { new() { Volume = "100 uL" } };

        var volumes = ResultEntryPresenter.VolumesFor(test, catalog);

        Assert.Equal(new[] { "850 uL" }, volumes.Select(v => v.Volume));
    }

    [Fact]
    public void VolumesFor_TestWithoutAllowedVolumes_FallsBackToCatalog()
    {
        var test = new TestType();
        var catalog = new List<SampleVolume> { new() { Volume = "100 uL" }, new() { Volume = "200 uL" } };

        var volumes = ResultEntryPresenter.VolumesFor(test, catalog);

        Assert.Equal(new[] { "100 uL", "200 uL" }, volumes.Select(v => v.Volume));
    }

    [Fact]
    public void VolumesFor_NullTest_FallsBackToCatalog()
    {
        var catalog = new List<SampleVolume> { new() { Volume = "500 uL" } };

        var volumes = ResultEntryPresenter.VolumesFor(null, catalog);

        Assert.Equal(new[] { "500 uL" }, volumes.Select(v => v.Volume));
    }

    [Fact]
    public void VolumesFor_SelectedSampleTypeWithAllowedVolumes_UsesThem()
    {
        var test = new TestType { AllowedVolumes = { new SampleVolume { Volume = "850 uL" } } };
        var sampleType = new SampleType
        {
            AllowedVolumes = { new SampleVolume { Volume = "150 uL" } },
        };
        var catalog = new List<SampleVolume> { new() { Volume = "500 uL" } };

        var volumes = ResultEntryPresenter.VolumesFor(test, sampleType, catalog);

        Assert.Equal(new[] { "150 uL" }, volumes.Select(v => v.Volume));
    }

    // --- SampleTypesFor -----------------------------------------------------

    [Fact]
    public void SampleTypesFor_TestWithAllowedSampleTypes_UsesThem()
    {
        var test = new TestType
        {
            AllowedSampleTypes =
            {
                new SampleType { DisplayName = "Whole Blood", Hl7Code = "BLD" },
            },
        };
        var catalog = new List<SampleType> { new() { DisplayName = "Plasma", Hl7Code = "PLAS" } };

        var sampleTypes = ResultEntryPresenter.SampleTypesFor(test, catalog);

        var sample = Assert.Single(sampleTypes);
        Assert.Equal("BLD", sample.Hl7Code);
    }

    [Fact]
    public void SampleTypesFor_TestWithoutAllowedSampleTypes_FallsBackToCatalog()
    {
        var test = new TestType();
        var catalog = new List<SampleType> { new() { DisplayName = "Plasma", Hl7Code = "PLAS" } };

        var sampleTypes = ResultEntryPresenter.SampleTypesFor(test, catalog);

        var sample = Assert.Single(sampleTypes);
        Assert.Equal("PLAS", sample.Hl7Code);
    }

    // --- EffectiveResultValue ----------------------------------------------

    [Fact]
    public void EffectiveResultValue_PrefersSelection()
    {
        var test = TestWith("POS", "NEG");

        Assert.Equal("NEG", ResultEntryPresenter.EffectiveResultValue(test, "NEG"));
    }

    [Fact]
    public void EffectiveResultValue_FallsBackToFirstValue_WhenNoSelection()
    {
        var test = TestWith("POS", "NEG");

        Assert.Equal("POS", ResultEntryPresenter.EffectiveResultValue(test, null));
        Assert.Equal("POS", ResultEntryPresenter.EffectiveResultValue(test, "  "));
    }

    [Fact]
    public void EffectiveResultValue_NA_WhenNothingAvailable()
    {
        Assert.Equal("N/A", ResultEntryPresenter.EffectiveResultValue(TestWith(), null));
        Assert.Equal("N/A", ResultEntryPresenter.EffectiveResultValue(null, null));
    }

    // --- CanSend ------------------------------------------------------------

    [Fact]
    public void CanSend_TrueWithTargetAndSampleType()
    {
        Assert.True(ResultEntryPresenter.CanSend(TestWith("POS"), new SampleType()));
    }

    [Fact]
    public void CanSend_FalseWithoutSampleType()
    {
        Assert.False(ResultEntryPresenter.CanSend(TestWith("POS"), null));
    }

    [Fact]
    public void CanSend_FalseWithoutTargets()
    {
        Assert.False(ResultEntryPresenter.CanSend(new TestType(), new SampleType()));
    }

    [Fact]
    public void CanSend_FalseWithoutTest()
    {
        Assert.False(ResultEntryPresenter.CanSend(null, new SampleType()));
    }
}
