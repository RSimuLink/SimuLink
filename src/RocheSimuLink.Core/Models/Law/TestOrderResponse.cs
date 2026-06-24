namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// One ORDER group within a response to a test order submission (ORL^O34):
    /// the instrument's per-order ORC reply.
    /// </summary>
    public sealed class TestOrderResponseItem
    {
        /// <summary>
        /// ORC-1 order control reply: "OK" accepted, "UA" unable to accept (for
        /// a NW order), "CR" canceled as requested (for a CA order).
        /// </summary>
        public string OrderControl { get; set; } = "OK";

        /// <summary>ORC-2 placer order number, copied from the OML OBR-2.</summary>
        public string PlacerOrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// ORC-5 order status: "SC"/"IP" when accepted, "CA" when unable or canceled.
        /// </summary>
        public string OrderStatus { get; set; } = "SC";
    }

    /// <summary>
    /// A response to a test order submission (ORL^O34): the instrument
    /// acknowledges the OML^O33 (MSA), echoes the specimen (SPM/SAC), and
    /// reports per-order acceptance (ORC) per the Host Interface Manual.
    ///
    /// The "unavailable test order" variant carries only MSH + MSA.
    /// </summary>
    public sealed class TestOrderResponse
    {
        /// <summary>The MSH context for the response message.</summary>
        public LawMessageHeader Header { get; set; } = new();

        /// <summary>MSA-1 acknowledgment code ("AA", "AR", "AE").</summary>
        public string AcknowledgmentCode { get; set; } = "AA";

        /// <summary>MSA-2 message control id of the OML being acknowledged.</summary>
        public string AcknowledgedControlId { get; set; } = string.Empty;

        /// <summary>
        /// The echoed specimen, or null for the MSH+MSA-only "unavailable" variant.
        /// </summary>
        public Specimen? Specimen { get; set; } = new();

        /// <summary>The per-order ORC replies.</summary>
        public List<TestOrderResponseItem> Orders { get; set; } = new();
    }
}
