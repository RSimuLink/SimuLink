namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// One ERR segment in a negative result acknowledgment, locating and coding
    /// a problem found in the OUL^R22. Present only when MSA-1 is not "AA".
    /// </summary>
    public sealed class AcknowledgmentError
    {
        /// <summary>ERR-2-1 segment id where the error was found (e.g. "OBX").</summary>
        public string SegmentId { get; set; } = string.Empty;

        /// <summary>ERR-2-2 segment sequence (e.g. "1").</summary>
        public string SegmentSequence { get; set; } = string.Empty;

        /// <summary>ERR-2-3 field number (e.g. "2"), optional.</summary>
        public string FieldNumber { get; set; } = string.Empty;

        /// <summary>ERR-3 error code, from HL70357 (e.g. "207").</summary>
        public CodedElement ErrorCode { get; set; } = new();

        /// <summary>ERR-4 severity (fixed "E" error).</summary>
        public string Severity { get; set; } = "E";

        /// <summary>ERR-8 user message (e.g. "Unknown value type"), optional.</summary>
        public string UserMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// A result acknowledgment (ACK^R22): the host's reply to an OUL^R22 test
    /// result. Carries MSA and, when negative, one or more ERR segments per the
    /// Host Interface Manual.
    /// </summary>
    public sealed class ResultAcknowledgment
    {
        /// <summary>The MSH context for the acknowledgment message.</summary>
        public LawMessageHeader Header { get; set; } = new();

        /// <summary>
        /// MSA-1 acknowledgment code: "AA" accepted, "AR" rejected, "AE" error.
        /// </summary>
        public string AcknowledgmentCode { get; set; } = "AA";

        /// <summary>MSA-2 message control id of the OUL^R22 being acknowledged.</summary>
        public string AcknowledgedControlId { get; set; } = string.Empty;

        /// <summary>ERR segments, present only when the acknowledgment is negative.</summary>
        public List<AcknowledgmentError> Errors { get; set; } = new();
    }
}
