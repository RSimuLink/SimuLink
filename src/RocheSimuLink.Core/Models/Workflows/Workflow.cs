namespace RocheSimuLink.Models.Workflows
{
    /// <summary>
    /// A single HL7 workflow the instrument supports, as declared by the host interface manual.
    /// </summary>
    public class Workflow
    {
        /// <summary>The workflow kind.</summary>
        public SupportedWorkflows Kind { get; set; }

        /// <summary>HL7 message type code (e.g. "OUL^R22").</summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>Human-readable description shown in the UI.</summary>
        public string Description { get; set; } = string.Empty;
    }
}
