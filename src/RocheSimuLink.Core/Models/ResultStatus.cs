namespace RocheSimuLink.Models
{
    /// <summary>
    /// Result status reported in OBX-11 (Observation Result Status).
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>Preliminary result (HL7 code "P").</summary>
        Preliminary,

        /// <summary>Final result (HL7 code "F").</summary>
        Final,

        /// <summary>Corrected result (HL7 code "C").</summary>
        Corrected,

        /// <summary>Result cannot be obtained for this observation (HL7 code "X").</summary>
        CannotObtain,
    }

    public static class ResultStatusExtensions
    {
        /// <summary>Maps the status to its HL7 OBX-11 code.</summary>
        public static string ToHl7Code(this ResultStatus status) => status switch
        {
            ResultStatus.Preliminary => "P",
            ResultStatus.Final => "F",
            ResultStatus.Corrected => "C",
            ResultStatus.CannotObtain => "X",
            _ => "F",
        };
    }
}
