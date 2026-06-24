namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// The host's response to a work order step query (RSP^K11). It acknowledges
    /// the QBP^Q11 (MSA), reports the query outcome (QAK), and echoes the query
    /// parameters (QPD) per the Host Interface Manual.
    /// </summary>
    public sealed class WorkOrderQueryResponse
    {
        /// <summary>The MSH context for the response message.</summary>
        public LawMessageHeader Header { get; set; } = new();

        /// <summary>MSA-1 acknowledgment code ("AA", "AR", "AE").</summary>
        public string AcknowledgmentCode { get; set; } = "AA";

        /// <summary>MSA-2 message control id of the QBP being acknowledged.</summary>
        public string AcknowledgedControlId { get; set; } = string.Empty;

        /// <summary>QAK-1 query tag, echoed from the request QPD-2.</summary>
        public string QueryTag { get; set; } = string.Empty;

        /// <summary>
        /// QAK-2 query response status: "OK" (data found), "NF" (no data found),
        /// "AE" (application error), "AR" (application reject).
        /// </summary>
        public string QueryResponseStatus { get; set; } = "OK";

        /// <summary>QAK-3 / QPD-1 query name, echoed from the request.</summary>
        public CodedElement QueryName { get; set; } =
            new("WOS_ROCHE", "Work Order Step Roche Extension", "ROCHE");

        /// <summary>QPD-2 query tag, echoed from the request.</summary>
        public string EchoedQueryTag { get; set; } = string.Empty;

        /// <summary>QPD-3 sample id, echoed from the request.</summary>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>QPD-4 carrier id, echoed from the request.</summary>
        public string CarrierId { get; set; } = string.Empty;

        /// <summary>QPD-5 carrier position, echoed from the request.</summary>
        public string CarrierPosition { get; set; } = string.Empty;
    }
}
