using RocheLIT.HL7.Law;
using RocheLIT.HL7.Parsers;
using RocheLIT.Him;
using RocheLIT.Models;
using RocheLIT.Models.Law;
using Xunit;

namespace RocheLIT.Core.Tests.Him;

/// <summary>
/// End-to-end thin slice: ingest the HIM PDF, persist and reload the portable
/// definitions file, map the catalog onto the UI models, and build a real
/// OUL^R22 result message for an assay pulled straight from the manual.
/// </summary>
public class HimEndToEndTests
{
    [Fact]
    public void IngestToDefinitionsFile_ReloadsIdentically()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"him_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmp);
        try
        {
            var (manual, path) = HimDefinitionsStore.IngestPdf(
                HimTestData.PdfPath, number: 1, outputDirectory: tmp);

            // File follows the HIMdefinitions_00x.txt convention.
            Assert.Equal("HIMdefinitions_001.txt", Path.GetFileName(path));
            Assert.True(HimDefinitionsStore.IsDefinitionsFileName(path));
            Assert.True(File.Exists(path));

            // Reloading the portable file yields the same catalog (no PDF needed).
            var reloaded = HimDefinitionsStore.Load(path);
            Assert.Equal(manual.Assays.Count, reloaded.Assays.Count);
            Assert.Equal(manual.MessageTypes.Count, reloaded.MessageTypes.Count);
            Assert.Equal(manual.ManualVersion, reloaded.ManualVersion);
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void CatalogMapsToUiModels()
    {
        var manual = HimParser.Parse(
            HimPdfReader.ReadPages(HimTestData.PdfPath), HimTestData.PdfFileName);

        var sampleTypes = HimCatalogMapper.ToSampleTypes(manual);
        var testTypes = HimCatalogMapper.ToTestTypes(manual);

        // Specimen catalog is de-duplicated by HL7 code and non-empty.
        Assert.NotEmpty(sampleTypes);
        Assert.Contains(sampleTypes, s => s.Hl7Code == "PLAS");
        Assert.Equal(sampleTypes.Select(s => s.Hl7Code),
            sampleTypes.Select(s => s.Hl7Code).Distinct());

        // The full SPM-4 coded element is retained (id^text^system), not just the id.
        Assert.Contains(sampleTypes, s => s.SpecimenCode == "PLAS^plasma^HL70487");

        // The CT/NG panel test mapped through with its two targets.
        var ctng = Assert.Single(testTypes, t =>
            t.UniversalServiceIdentifier == "72828-7^CT/NG^LN");
        Assert.Equal(2, ctng.Targets.Count);
        Assert.Contains(ctng.Targets, t => t.ObservationIdentifier == "CT^CT^99ROC");
        Assert.Contains(ctng.Targets, t => t.ObservationIdentifier == "NG^NG^99ROC");
        Assert.Contains(ctng.AllowedVolumes, v => v.Volume == "850 uL");
        Assert.Contains(ctng.AllowedSampleTypes, s => s.Hl7Code == "UR");
        Assert.Contains(ctng.AllowedSampleTypes, s => s.Hl7Code == "SWAB");
        Assert.DoesNotContain(ctng.AllowedSampleTypes, s => s.Hl7Code == "PLAS");

        var malaria = Assert.Single(testTypes, t =>
            t.UniversalServiceIdentifier == "50687-3^Malaria^LN");
        var malariaSample = Assert.Single(malaria.AllowedSampleTypes);
        Assert.Equal("BLD", malariaSample.Hl7Code);
        Assert.Equal("500 uL", Assert.Single(malariaSample.AllowedVolumes).Volume);

        var hiv1 = Assert.Single(testTypes, t =>
            t.Name == "HIV-1" && t.UniversalServiceIdentifier == "70241-5^HIV^LN");
        var hiv1Sample = Assert.Single(hiv1.AllowedSampleTypes);
        Assert.Equal("PLASMA", hiv1Sample.DisplayName);
        Assert.Equal(new[] { "200 uL", "500 uL" },
            hiv1Sample.AllowedVolumes.Select(v => v.Volume));
    }

    [Fact]
    public void BuildsOulR22FromManualSourcedAssay()
    {
        var manual = HimParser.Parse(
            HimPdfReader.ReadPages(HimTestData.PdfPath), HimTestData.PdfFileName);

        // Pull the CT/NG panel and a plasma/urine specimen straight from the HIM.
        var testTypes = HimCatalogMapper.ToTestTypes(manual);
        var ctng = testTypes.First(t => t.UniversalServiceIdentifier == "72828-7^CT/NG^LN");
        var sampleType = HimCatalogMapper.ToSampleTypes(manual)
            .First(s => s.Hl7Code == "UR");

        var settings = new ConnectionSettings
        {
            SendingApplication = "X800DM",
            ReceivingApplication = "Host",
        };

        var resultMessage = LawResultMessageFactory.Create(
            "SID-CTNG-1", sampleType, ctng, ctng.Targets[0], "Positive",
            ResultFlag.High, settings,
            new DateTimeOffset(2024, 10, 29, 17, 29, 20, TimeSpan.FromHours(1)));

        var built = LawOulR22Builder.Build(resultMessage);
        var parsed = Hl7Parser.Parse(built.RawMessage);

        Assert.Equal("OUL^R22", parsed.MessageType);
        // One OBX per CT/NG target (multi-channel) sourced from the manual.
        Assert.Equal(2, parsed.AllSegments("OBX").Count());
        // SPM-4 carries the full coded element from the manual, not just the id.
        var spm4 = parsed.Segment("SPM")!.Field(4);
        Assert.Equal("UR^Urine^HL70487", spm4);
        // OBX-3 carries the manual's observation identifiers.
        var obxIds = parsed.AllSegments("OBX").Select(o => o.Component(3, 1)).ToList();
        Assert.Contains("CT", obxIds);
        Assert.Contains("NG", obxIds);
    }
}
