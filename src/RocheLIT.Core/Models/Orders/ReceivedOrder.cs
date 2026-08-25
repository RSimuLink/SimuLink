namespace RocheLIT.Models.Orders
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

        /// <summary>Requested test type summary from OBR-4.</summary>
        public string TestType { get; set; } = string.Empty;

        /// <summary>Specimen type received in SPM-4.</summary>
        public string SampleType { get; set; } = string.Empty;

        /// <summary>Sample/consumption volume received in TCD-9.</summary>
        public string SampleVolume { get; set; } = string.Empty;

        /// <summary>Container carrier id received in SAC-10.</summary>
        public string CarrierId { get; set; } = string.Empty;

        /// <summary>Container carrier position received in SAC-11.</summary>
        public string CarrierPosition { get; set; } = string.Empty;

        /// <summary>Tests requested in this order.</summary>
        public List<OrderedTest> Tests { get; set; } = new();

        /// <summary>The HL7 message type that delivered the order (e.g. "OML^O33").</summary>
        public string MessageType { get; set; } = string.Empty;
    }
}
