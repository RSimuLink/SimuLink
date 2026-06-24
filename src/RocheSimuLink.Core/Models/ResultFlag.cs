namespace RocheSimuLink.Models
{
    /// <summary>
    /// Abnormal-flags reported in OBX-8 (Abnormal Flags). These map to the
    /// mockup's Normal / High / Low / Critical checkboxes.
    /// </summary>
    public enum ResultFlag
    {
        /// <summary>Within normal limits (HL7 code "N").</summary>
        Normal,

        /// <summary>Above high normal (HL7 code "H").</summary>
        High,

        /// <summary>Below low normal (HL7 code "L").</summary>
        Low,

        /// <summary>Critical / panic value (HL7 code "AA" = critically abnormal).</summary>
        Critical,
    }

    public static class ResultFlagExtensions
    {
        /// <summary>Maps the flag to its HL7 OBX-8 code.</summary>
        public static string ToHl7Code(this ResultFlag flag) => flag switch
        {
            ResultFlag.Normal => "N",
            ResultFlag.High => "H",
            ResultFlag.Low => "L",
            ResultFlag.Critical => "AA",
            _ => "N",
        };
    }
}
