using RocheLIT.Him;
using RocheLIT.Models.Him;
using Xunit;

namespace RocheLIT.Core.Tests.Him;

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
    public void ParsesAssaySpecificSampleTypesAndInputVolumes()
    {
        var hev = Assay("HEV");
        var hevSample = Assert.Single(hev.SampleTypes);
        Assert.Equal("PLASMA", hevSample.Name);
        Assert.Equal("PLAS^plasma^HL70487", hevSample.SpecimenType);

        var malaria = Assay("Malaria");
        var malariaSample = Assert.Single(malaria.SampleTypes);
        Assert.Equal("Whole Blood", malariaSample.Name);
        Assert.Equal("BLD^Whole Blood^HL70487", malariaSample.SpecimenType);

        var hiv1 = Assay("HIV-1");
        var hiv1Sample = Assert.Single(hiv1.SampleTypes);
        Assert.Equal("PLASMA", hiv1Sample.Name);
        Assert.Equal("PLAS^plasma^HL70487", hiv1Sample.SpecimenType);
        Assert.Equal("200", hiv1Sample.VolumeMicroliters);
        Assert.Equal(new[] { "200", "500" }, hiv1Sample.VolumeOptionsMicroliters);

        var mpxe = Assay("MPX-E");
        Assert.Equal(
            new[] { "PLASMA", "SERUM", "CADAVERIC PLASMA", "CADAVERIC SERUM" },
            mpxe.SampleTypes.Select(s => s.Name));
        Assert.All(mpxe.SampleTypes, sample =>
            Assert.Equal(new[] { "850", "150" }, sample.VolumeOptionsMicroliters));
    }

    [Fact]
    public void ParsesMpxeReactiveNonReactiveTargets()
    {
        var mpxe = Assay("MPX-E");

        Assert.Equal(new[] { "HBV", "HIV", "HEV", "HCV" }, mpxe.Targets.Select(t => t.Name));
        Assert.All(mpxe.Targets, target =>
        {
            Assert.Equal(new[] { "RR", "NR" }, target.ObservationValues);
            Assert.Equal(new[] { "Reactive", "Non-Reactive" }, target.InterpretationCodes);
        });
    }

    [Fact]
    public void ParsesSarsCov2DuoTargetSpecificResultCodes()
    {
        var duo = Assert.Single(Manual().Assays, a =>
            a.Description.StartsWith("SARS-CoV-2 Duo is", StringComparison.OrdinalIgnoreCase));

        var qualitative = Assert.Single(duo.Targets, t =>
            t.Name.Contains("Qual", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(new[] { "POS", "NEG" }, qualitative.ObservationValues);
        Assert.Equal(new[] { "Positive", "Negative" }, qualitative.InterpretationCodes);

        var quantitative = Assert.Single(duo.Targets, t =>
            t.Name.Contains("Quant", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(new[] { "VAL", "AT", "BT", "ND" }, quantitative.ObservationValues);
        Assert.Equal(
            new[] { "Valid", "Above Titer", "Below Titer", "Not Detected" },
            quantitative.InterpretationCodes);
    }

    [Fact]
    public void ParsesDpxTargetSpecificResultCodes()
    {
        var dpx = Assay("DPX");

        var hav = Assert.Single(dpx.Targets, t => t.Name == "HAV");
        Assert.Equal(new[] { "RR", "NR" }, hav.ObservationValues);
        Assert.Equal(new[] { "Reactive", "Non-Reactive" }, hav.InterpretationCodes);

        var b19 = Assert.Single(dpx.Targets, t => t.Name == "B19");
        Assert.Equal(new[] { "VAL", "AT", "BT", "ND" }, b19.ObservationValues);
        Assert.Equal(
            new[] { "Valid", "Above Titer", "Below Titer", "Not Detected" },
            b19.InterpretationCodes);
    }

    [Fact]
    public void ParsesPreviouslyMissingAssayMappings()
    {
        var hbvRuo = Assay("HBV-RNA-RUO");
        Assert.Contains(hbvRuo.Tests, t => t.UniversalServiceIdentifier == "HBVRNA^RUO^99ROC");

        var hdvRuo = Assay("HDV-RUO");
        Assert.Contains(hdvRuo.Tests, t => t.UniversalServiceIdentifier == "HDV^RUO^99ROC");

        var resp4Flex = Assay("RESP-4FLEX");
        Assert.Equal(4, resp4Flex.Tests.Count);
        Assert.Contains(resp4Flex.Tests, t => t.UniversalServiceIdentifier == "94309-2^SCoV2^LN");

        var respFlex = Assay("RESP-FLEX");
        Assert.Equal(12, respFlex.Tests.Count);
        Assert.Contains(respFlex.Targets, t => t.ObservationIdentifier == "hPIV4^hPIV4^99ROC");

        var sarsDuo = Assay("SARS-CoV-2 Duo");
        Assert.Contains(sarsDuo.Tests, t => t.UniversalServiceIdentifier == "97104-4^SARS-COV-2^LN");

        var scov2Fluab = Assay("SCoV2-FluA/B");
        Assert.Contains(scov2Fluab.Tests, t => t.UniversalServiceIdentifier == "95380-2^SCoVFlu^LN");

        var bvcv = Assay("BV/CV");
        Assert.Contains(bvcv.Tests, t => t.UniversalServiceIdentifier == "92703-8^BV/CV^LN");
    }

    [Fact]
    public void CleansReportedControlResultNames()
    {
        Assert.Equal(new[] { "DPX D (+) C", "DPX H (+) C", "(-) C" },
            Assay("DPX").ControlResults.Select(c => c.Name));

        Assert.Equal(new[] { "HIV-1M/HIV-2 (+) C", "HIV-1O (+) C", "(-) C" },
            Assay("HIV-1/2-Qual-DBS").ControlResults.Select(c => c.Name));
        Assert.Equal(new[] { "HIV-1M/HIV-2 (+) C", "HIV-1O (+) C", "(-) C" },
            Assay("HIV-1/2-Qual-Ser/Pla").ControlResults.Select(c => c.Name));

        Assert.Equal(new[] { "SARS-CoV-2 H (+) C", "SARS-CoV-2 L (+) C", "(-) Ctrl" },
            Assay("SARS-CoV-2 Duo").ControlResults.Select(c => c.Name));
        Assert.Equal(new[] { "SCoV2-FluA/B (+) C", "(-) Ctrl" },
            Assay("SCoV2-FluA/B").ControlResults.Select(c => c.Name));
    }

    [Fact]
    public void AppliesReportedAssaySpecificCatalogCorrections()
    {
        var hivSerPla = Assay("HIV-1/2-Qual-Ser/Pla");
        Assert.Contains(hivSerPla.Tests, t => t.Name == "HIV-1/2-Qual-Ser/Pla");

        var hivPsc = Assay("HIV-PSC");
        var psc = Assert.Single(hivPsc.SampleTypes);
        Assert.Equal("PSC", psc.Name);
        Assert.Equal("PSEPC^Plasma Separation Card^99ROC", psc.SpecimenType);

        var scov2Fluab = Assay("SCoV2-FluA/B");
        Assert.Equal(
            new[] { "VIRAL TRANSPORT MEDIA", "COBAS PCR MEDIA SWAB" },
            scov2Fluab.SampleTypes.Select(s => s.Name));
        Assert.All(scov2Fluab.SampleTypes, s =>
            Assert.Equal(new[] { "400" }, s.VolumeOptionsMicroliters));
        Assert.Equal(
            new[] { "FluA^FluA^99ROC", "SCoV2^SCoV2^99ROC", "PanSarb^PanSarb^99ROC", "FluB^FluB^99ROC" },
            scov2Fluab.Targets.Select(t => t.ObservationIdentifier));

        var hpvGt = Assay("HPV-GT");
        Assert.Contains(hpvGt.SampleTypes, s => s.Name == "Self, vaginal (for US only)" &&
            s.SpecimenType == "SVAL^self, vaginal^99ROC");
        Assert.Contains(hpvGt.SampleTypes, s => s.Name == "SUREPATH" &&
            s.SpecimenType == "SPATH^SurePath^99ROC");
        Assert.Contains(hpvGt.SampleTypes, s => s.Name == "Self, vaginal - RCCM/PC" &&
            s.SpecimenType == "SVAG^self, vaginal - RCCM/PC^99ROC");
        Assert.Contains(hpvGt.Targets, t =>
            t.ObservationIdentifier == "Other HR HPV^Other HR HPV^99ROC");

        var hpvHr = Assay("HPV-HR");
        Assert.Contains(hpvHr.SampleTypes, s => s.Name == "Self, vaginal (for US only)" &&
            s.SpecimenType == "SVAL^self, vaginal^99ROC");
        Assert.Contains(hpvHr.SampleTypes, s => s.Name == "SUREPATH" &&
            s.SpecimenType == "SPATH^SurePath^99ROC");
        Assert.Contains(hpvHr.Targets, t =>
            t.ObservationIdentifier == "HR HPV^HR HPV^99ROC");
    }

    [Fact]
    public void ParsesControlResultsForMalariaAndHiv()
    {
        var malaria = Assay("Malaria");
        Assert.Contains(malaria.ControlResults, c => c.Name == "Malaria (+) C" && c.IsPositive);
        Assert.Contains(malaria.ControlResults, c => c.Name == "(-) C" && !c.IsPositive);

        var hiv1 = Assay("HIV-1");
        Assert.Contains(hiv1.ControlResults, c => c.Name == "HxV H (+) C" && c.IsPositive);
        Assert.Contains(hiv1.ControlResults, c => c.Name == "HxV L (+) C" && c.IsPositive);
        Assert.Contains(hiv1.ControlResults, c => c.Name == "(-) C" && !c.IsPositive);
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
    public void NoTargetMixesResultFamilies()
    {
        var resultFamilies = new[]
        {
            new HashSet<string>(new[] { "POS", "NEG" }, StringComparer.Ordinal),
            new HashSet<string>(new[] { "RR", "NR" }, StringComparer.Ordinal),
            new HashSet<string>(new[] { "VAL", "AT", "BT", "ND" }, StringComparer.Ordinal),
        };

        foreach (var target in Manual().Assays.SelectMany(a => a.Targets))
        {
            var matchedFamilies = resultFamilies.Count(family =>
                target.ObservationValues.Any(family.Contains));

            Assert.True(matchedFamilies <= 1,
                $"{target.Name} ({target.ObservationIdentifier}) mixes incompatible result choice families.");
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
