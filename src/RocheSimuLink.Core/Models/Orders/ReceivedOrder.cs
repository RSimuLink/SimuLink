namespace RocheSimuLink.Models.Orders
{
    /// <summary>
    /// An order received from the LIS, projected into the fields shown in the
    /// "Received LIS Order Details" panel.
    /// </summary>
    public sealed class ReceivedOrder
    {
        /// <summary>Placer/filler order number (ORC-2 / ORC-3).</summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>Specimen identifier (SPM-2 or OBR-3).</summary>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>Patient demographics.</summary>
        public Patient Patient { get; set; } = new();

        /// <summary>Tests requested in this order.</summary>
        public List<OrderedTest> Tests { get; set; } = new();

        /// <summary>The HL7 message type that delivered the order (e.g. "OML^O33").</summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>The raw HL7 text, retained for the activity log / inspection.</summary>
        public string RawMessage { get; set; } = string.Empty;
    }
}
