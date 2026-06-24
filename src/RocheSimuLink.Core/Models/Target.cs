namespace RocheSimuLink.Models
{
    /// <summary>
    /// A measurable target within a test (e.g. an analyte/marker that produces an OBX result).
    /// </summary>
    public class Target
    {
        /// <summary>Human-readable target name shown in the UI.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>OBX-3 Observation Identifier (e.g. LOINC or vendor code).</summary>
        public string ObservationIdentifier { get; set; } = string.Empty;

        /// <summary>Candidate observation values used to populate OBX-5.</summary>
        public List<string> ObservationValues { get; set; } = new();

        /// <summary>Candidate interpretation codes used to populate OBX-8 (e.g. N, H, L, A).</summary>
        public List<string> InterpretationCodes { get; set; } = new();
    }
}
