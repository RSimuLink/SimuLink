namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// A work order step query (QBP^Q11): the instrument asks the LIS whether a
    /// scanned sample has tests to run. Carries the QPD query parameters and the
    /// RCP response-control settings from the Host Interface Manual.
    /// </summary>
    public sealed class WorkOrderQuery
    {
        /// <summary>The MSH context for the query message.</summary>
        public LawMessageHeader Header { get; set; } = new();

        /// <summary>
        /// QPD-1 query name. Fixed for the Roche work-order-step extension.
        /// </summary>
        public CodedElement QueryName { get; set; } =
            new("WOS_ROCHE", "Work Order Step Roche Extension", "ROCHE");

        /// <summary>QPD-2 query tag (a GUID), echoed back in RSP QAK-1/QPD-2.</summary>
        public string QueryTag { get; set; } = string.Empty;

        /// <summary>QPD-3 container/sample id (the scanned barcode).</summary>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>QPD-4 carrier/rack id, matched against SAC-10.</summary>
        public string CarrierId { get; set; } = string.Empty;

        /// <summary>QPD-5 rack position, matched against SAC-11.</summary>
        public string CarrierPosition { get; set; } = string.Empty;

        /// <summary>RCP-1 query priority (e.g. "I" immediate).</summary>
        public string QueryPriority { get; set; } = "I";

        /// <summary>RCP-3 response modality (fixed "R^Real Time^HL70394").</summary>
        public CodedElement ResponseModality { get; set; } =
            new("R", "Real Time", "HL70394");
    }
}
