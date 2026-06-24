using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds the SPM (sample) and SAC (container) segments shared by the
    /// result (OUL^R22), order (OML^O33), and order response (ORL^O34) flows.
    /// </summary>
    public static class LawSpecimenBuilder
    {
        public static string BuildSpm(Specimen s) => new LawField("SPM")
            .Set(1, "1")
            .Set(2, s.EntityIdentifier)
            .Set(4, s.SpecimenType.ToHl7())
            .Set(11, s.Role.Length > 0 ? $"{s.Role}^^HL70369" : string.Empty)
            .Render();

        public static string BuildSac(Specimen s) => new LawField("SAC")
            .Set(3, s.SampleId)
            .Set(10, s.CarrierId)
            .Set(11, s.CarrierPosition)
            .Render();
    }
}
