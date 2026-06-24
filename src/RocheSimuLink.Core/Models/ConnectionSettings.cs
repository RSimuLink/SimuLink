namespace RocheSimuLink.Models
{
    /// <summary>
    /// LIS connection and HL7 identity settings exposed by the Settings dialog.
    /// </summary>
    public sealed class ConnectionSettings
    {
        /// <summary>LIS host the simulator sends results to.</summary>
        public string LisHost { get; set; } = "127.0.0.1";

        /// <summary>LIS port the simulator sends results to.</summary>
        public int LisPort { get; set; } = 5000;

        /// <summary>Local port the simulator listens on for inbound LIS orders.</summary>
        public int ListenPort { get; set; } = 5001;

        /// <summary>MSH-3 Sending Application.</summary>
        public string SendingApplication { get; set; } = "SimuLink";

        /// <summary>MSH-4 Sending Facility.</summary>
        public string SendingFacility { get; set; } = "Roche";

        /// <summary>MSH-5 Receiving Application.</summary>
        public string ReceivingApplication { get; set; } = "LIS";

        /// <summary>MSH-6 Receiving Facility.</summary>
        public string ReceivingFacility { get; set; } = "Hospital";

        /// <summary>HL7 version reported in MSH-12.</summary>
        public string Hl7Version { get; set; } = "2.5.1";

        // Reserved for the future host-interface-manual ingestion milestone.
        // The Settings dialog can surface this once PDF parsing lands.
        /// <summary>Optional path to the host interface manual PDF (future use).</summary>
        public string? HostInterfaceManualPath { get; set; }
    }
}
