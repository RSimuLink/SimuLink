using RocheLIT.Models;

namespace RocheLIT.Models.Ucap
{
    /// <summary>UCAP-specific static catalog from HIM 5.3 pages 190-191.</summary>
    public static class UcapCatalog
    {
        public static List<SampleType> SampleTypes() => new()
        {
            Sample("COBAS PCR MEDIA SWAB", "CPM^cobas PCR Media^99ROC", "400"),
            Sample("Diluted in cobas MIS", "DCMIS^diluted in cobas MIS^99ROC", "850"),
            Sample("PLASMA", "PLAS^plasma^HL70487", "200", "350", "500", "800"),
            Sample("PRESERVCYT", "PCYT^preservCyt^99ROC", "400"),
            Sample("ROCHE CELL COLLECTION MEDIA", "RCCM^RocheCellCollectionMedia^99ROC", "400"),
            Sample("Raw sputum", "SPTR^rawSputum^99ROC", "850"),
            Sample("Sediment", "SED^Sediment^99ROC", "850"),
            Sample("SERUM", "SER^serum^HL70487", "200", "500", "850"),
            Sample("SWAB", "SWAB^Swab^99ROC", "400"),
            Sample("U_Alcohol-based sample", "UCAlcS^UC_Alcohol-based sample^99ROC", "150", "200", "400", "850"),
            Sample("U_Buffer-based sample", "UCBufS^UC_Buffer-based sample^99ROC", "150", "200", "400", "850"),
            Sample("U_Sample with swab", "UCSwabS^UC_Sample with swab^99ROC", "150", "200", "400", "850"),
            Sample("U_Simple sample", "UCSimpS^UC_Simple sample^99ROC", "150", "200", "350", "500", "850"),
            Sample("URINE", "UR^Urine^HL70487", "400", "850"),
            Sample("WHOLE BLOOD", "BLD^Whole Blood^HL70487", "500"),
            Sample("VIRAL TRANSPORT MEDIA", "VTM^Viral Transport Media^99ROC", "400"),
        };

        private static SampleType Sample(string displayName, string specimenCode, params string[] volumes) => new()
        {
            DisplayName = displayName,
            Hl7Code = specimenCode.Split('^')[0],
            SpecimenCode = specimenCode,
            AllowedVolumes = volumes
                .Select(v => new SampleVolume { Volume = $"{v} uL" })
                .ToList(),
        };
    }
}
