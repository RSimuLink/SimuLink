namespace RocheSimuLink.Models
{
    /// <summary>
    /// A specimen type (e.g. Serum, Plasma, Whole Blood) and its HL7 coding.
    /// </summary>
    public class SampleType
    {
        /// <summary>Human-readable name shown in the UI.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>HL7 specimen code used in SPM (e.g. SER, PLAS, BLD).</summary>
        public string Hl7Code { get; set; } = string.Empty;
    }
}
