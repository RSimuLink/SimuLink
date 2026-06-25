namespace RocheSimuLink.Models.Workflows
{
    /// <summary>
    /// One generated example: the workflow it came from, its display metadata,
    /// and the raw HL7 text. Returned by the Example Generator for display,
    /// copy, and file export.
    /// </summary>
    public sealed class GeneratedMessage
    {
        public GeneratedMessage(
            ExampleWorkflow workflow,
            string label,
            string messageType,
            string direction,
            string rawMessage)
        {
            Workflow = workflow;
            Label = label;
            MessageType = messageType;
            Direction = direction;
            RawMessage = rawMessage;
        }

        /// <summary>The workflow this message belongs to.</summary>
        public ExampleWorkflow Workflow { get; }

        /// <summary>Step label, e.g. "LAB-27 - Work order request".</summary>
        public string Label { get; }

        /// <summary>HL7 message type code, e.g. "QBP^Q11".</summary>
        public string MessageType { get; }

        /// <summary>Communication direction, e.g. "Instrument to LIS".</summary>
        public string Direction { get; }

        /// <summary>The raw HL7 text (segments separated by carriage returns).</summary>
        public string RawMessage { get; }
    }
}
