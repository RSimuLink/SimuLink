using RocheSimuLink.Him;
using RocheSimuLink.Models;
using Xunit;

namespace RocheSimuLink.Core.Tests.Him;

/// <summary>
/// Verifies importing the HIM catalog onto SimuLinkSettings, via both the PDF
/// and the portable definitions file, leaving connection settings intact.
/// </summary>
public class HimSettingsImporterTests
{
    [Fact]
    public void ImportFromPdf_ReplacesCatalog_KeepsConnection()
    {
        var settings = new SimuLinkSettings
        {
            Connection = new ConnectionSettings
            {
                LisHost = "10.0.0.5",
                LisPort = 5100,
                SendingApplication = "X800DM",
            },
        };

        var summary = HimSettingsImporter.ImportFrom(settings, HimTestData.PdfPath);

        Assert.Equal("5.3", summary.ManualVersion);
        Assert.True(summary.AssayCount >= 25);
        Assert.NotEmpty(settings.TestTypes);
        Assert.NotEmpty(settings.SampleTypes);
        Assert.NotEmpty(settings.SampleVolumes);
        Assert.Contains(settings.TestTypes, t =>
            t.UniversalServiceIdentifier == "72828-7^CT/NG^LN");

        // Connection settings are untouched by a catalog import.
        Assert.Equal("10.0.0.5", settings.Connection.LisHost);
        Assert.Equal(5100, settings.Connection.LisPort);
        Assert.Equal("X800DM", settings.Connection.SendingApplication);
    }

    [Fact]
    public void LoadManual_ChoosesParserByExtension()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"him_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmp);
        try
        {
            // Ingest PDF -> definitions file, then load the .txt back.
            var (_, defsPath) = HimDefinitionsStore.IngestPdf(
                HimTestData.PdfPath, number: 7, outputDirectory: tmp);

            var fromPdf = HimSettingsImporter.LoadManual(HimTestData.PdfPath);
            var fromDefs = HimSettingsImporter.LoadManual(defsPath);

            Assert.Equal(fromPdf.Assays.Count, fromDefs.Assays.Count);
            Assert.Equal(fromPdf.ManualVersion, fromDefs.ManualVersion);
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void SampleVolumes_AreDistinctAndNumericallySorted()
    {
        var manual = HimSettingsImporter.LoadManual(HimTestData.PdfPath);
        var volumes = HimCatalogMapper.ToSampleVolumes(manual)
            .Select(v => v.Volume)
            .ToList();

        Assert.Equal(volumes, volumes.Distinct());

        var numbers = volumes
            .Select(v => int.Parse(v.Replace(" uL", string.Empty)))
            .ToList();
        var sorted = numbers.OrderBy(n => n).ToList();
        Assert.Equal(sorted, numbers);
    }
}
