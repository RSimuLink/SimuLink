namespace RocheSimuLink.Models.Him
{
    /// <summary>
    /// A message type the host interface supports, discovered from the example
    /// messages in the manual (MSH-9 triple plus the bare code^trigger).
    /// </summary>
    public sealed class HimMessageType
    {
        /// <summary>Full MSH-9 triple (e.g. "QBP^Q11^QBP_Q11").</summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>Bare code^trigger (e.g. "QBP^Q11").</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// A representative example message captured from the manual, with
        /// segments separated by CR. Useful as a golden reference / template.
        /// </summary>
        public string ExampleMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// The structured result of ingesting an x800 Data Manager Host Interface
    /// Manual: the supported message types and the assay catalog. This is the
    /// content serialized to the portable HIMdefinitions_00x.txt file so other
    /// instances can load it without the PDF.
    /// </summary>
    public sealed class HostInterfaceManual
    {
        /// <summary>Schema version of this definitions document.</summary>
        public int SchemaVersion { get; set; } = 1;

        /// <summary>Manual version string, if detected (e.g. "5.3").</summary>
        public string ManualVersion { get; set; } = string.Empty;

        /// <summary>Source description (e.g. the PDF file name) for provenance.</summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>When the ingestion was performed (UTC, ISO-8601).</summary>
        public string IngestedAtUtc { get; set; } = string.Empty;

        /// <summary>Message types found in the manual's example messages.</summary>
        public List<HimMessageType> MessageTypes { get; set; } = new();

        /// <summary>The assay catalog from the "Assays" chapter.</summary>
        public List<AssayDefinition> Assays { get; set; } = new();
    }
}
