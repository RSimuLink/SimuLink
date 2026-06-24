using RocheSimuLink.Him;
using RocheSimuLink.Models.Him;
using Xunit;

namespace RocheSimuLink.Core.Tests.Him;

/// <summary>
/// Verifies the HIM parser against the real HIMv2_1.pdf in the repository.
/// Skipped automatically when the PDF is not present.
/// </summary>
public class HimParserTests
{
    private static HostInterfaceManual? _cache;

    private static HostInterfaceManual Manual()
    {
        // Parse once and reuse across facts (PdfPig over 212 pages is not free).
        return _cache ??= HimParser.Parse(
            HimPdfReader.ReadPages(HimTestData.PdfPath), HimTestData.PdfFileName);
    }

    private static AssayDefinition Assay(string name) =>
        Assert.Single(Manual().Assays, a =>
            string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void DetectsManualVersion()
    {
        Assert.Equal("5.3", Manual().ManualVersion);
    }

    [Fact]
    public void FindsTheFourSupportedMessageTypes()
    {

        var codes = Manual().MessageTypes.Select(m => m.Code).ToList();
        Assert.Contains("QBP^Q11", codes);
        Assert.Contains("RSP^K11", codes);
        Assert.Contains("OML^O33", codes);
        Assert.Contains("ORL^O34", codes);
    }

    [Fact]
    public void CapturesQbpExampleMessage()
    {

        var qbp = Assert.Single(Manual().MessageTypes, m => m.Code == "QBP^Q11");
        var segments = qbp.ExampleMessage.Split('\r');

        Assert.Equal("QBP^Q11^QBP_Q11", qbp.MessageType);
        Assert.StartsWith("MSH|^~\\&|X800 DM||HOST|", segments[0]);
        Assert.Contains("QBP^Q11^QBP_Q11", segments[0]);
        Assert.Contains("LAB-27R^ROCHE", segments[0]);
        Assert.StartsWith("QPD|WOS_ROCHE^Work Order Step Roche Extension^ROCHE", segments[1]);
        Assert.StartsWith("RCP|I||R^Real Time^HL70394", segments[2]);
    }

    [Fact]
    public void ParsesMultiTargetQualitativeAssay_CtNg()
    {

        var ctng = Assay("CT/NG");
        Assert.False(ctng.IsQuantitative);

        // Sample types and volumes (SPM-4 / TCD-9-1).
        Assert.Contains(ctng.SampleTypes, s =>
            s.SpecimenType == "UR^Urine^HL70487" && s.VolumeMicroliters == "850");
        Assert.Contains(ctng.SampleTypes, s => s.SpecimenType == "SWAB^Swab^99ROC");

        // Tests (OBR-4 / TCD-1): panel + per-channel.
        var tests = ctng.Tests.Select(t => t.UniversalServiceIdentifier).ToList();
        Assert.Contains("72828-7^CT/NG^LN", tests);
        Assert.Contains("21613-5^CT^LN", tests);
        Assert.Contains("24111-7^NG^LN", tests);

        // Targets (OBX-3).
        var targets = ctng.Targets.Select(t => t.ObservationIdentifier).ToList();
        Assert.Contains("CT^CT^99ROC", targets);
        Assert.Contains("NG^NG^99ROC", targets);

        // Result codes (OBX-5) and their interpretations (OBX-8-1): the
        // qualitative POS/NEG set applies to every target.
        Assert.All(ctng.Targets, t =>
        {
            Assert.Equal(new[] { "POS", "NEG" }, t.ObservationValues);
            Assert.Equal(new[] { "Positive", "Negative" }, t.InterpretationCodes);
        });
    }

    [Fact]
    public void ParsesSingleChannelQuantitativeAssay_Bkv()
    {

        var bkv = Assay("BKV");
        Assert.True(bkv.IsQuantitative);
        Assert.Contains(bkv.Tests, t => t.UniversalServiceIdentifier == "32284-2^BKV^LN");
        Assert.Contains(bkv.SampleTypes, s => s.SpecimenType == "PLAS^plasma^HL70487");

        // The quantitative titer result-code set (VAL/AT/BT/ND) on the target.
        var target = Assert.Single(bkv.Targets);
        Assert.Equal("BKV^BKV^99ROC", target.ObservationIdentifier);
        Assert.Equal(new[] { "VAL", "AT", "BT", "ND" }, target.ObservationValues);
        Assert.Equal(
            new[] { "Valid", "Above Titer", "Below Titer", "Not Detected" },
            target.InterpretationCodes);
    }

    [Fact]
    public void EveryParsedTargetHasMatchingValueAndInterpretationCounts()
    {
        foreach (var target in Manual().Assays.SelectMany(a => a.Targets))
        {
            Assert.Equal(
                target.ObservationValues.Count, target.InterpretationCodes.Count);
        }
    }

    [Fact]
    public void MostTargetsCarryResultCodes()
    {
        var targets = Manual().Assays.SelectMany(a => a.Targets).ToList();
        var withValues = targets.Count(t => t.ObservationValues.Count > 0);

        // The bulk of targets resolve result codes. A handful (control/
        // blood-screening assays whose code tables span a page boundary) do
        // not, which the page-scoped parser does not stitch together.
        Assert.True(withValues >= targets.Count * 0.8,
            $"Only {withValues}/{targets.Count} targets carry result codes.");
    }

    [Fact]
    public void IngestsAReasonableNumberOfAssays()
    {
        Assert.True(Manual().Assays.Count >= 25,
            $"Expected at least 25 assays, found {Manual().Assays.Count}.");
    }
}
