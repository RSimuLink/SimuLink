using System.Collections.Generic;

namespace RocheSimuLink.Models.Workflows
{
    /// <summary>
    /// Display metadata for each <see cref="ExampleWorkflow"/>: the step label,
    /// HL7 message type, and communication direction. Shared by the generator
    /// and the UI so the seven flows are described in one place.
    /// </summary>
    public sealed class ExampleWorkflowInfo
    {
        public ExampleWorkflowInfo(
            ExampleWorkflow workflow, string label, string messageType, string direction)
        {
            Workflow = workflow;
            Label = label;
            MessageType = messageType;
            Direction = direction;
        }

        public ExampleWorkflow Workflow { get; }

        /// <summary>Step label, e.g. "LAB-27 - Work order request".</summary>
        public string Label { get; }

        /// <summary>HL7 message type code, e.g. "QBP^Q11".</summary>
        public string MessageType { get; }

        /// <summary>Communication direction, e.g. "Instrument to LIS".</summary>
        public string Direction { get; }

        /// <summary>A one-line caption combining the parts for list display.</summary>
        public string Caption => $"{Label}  [{MessageType}]  ({Direction})";

        /// <summary>The seven supported flows, in conversation order.</summary>
        public static IReadOnlyList<ExampleWorkflowInfo> All { get; } = new[]
        {
            new ExampleWorkflowInfo(ExampleWorkflow.Lab27WorkOrderRequest,
                "LAB-27 - Work order request", "QBP^Q11", "Instrument to LIS"),
            new ExampleWorkflowInfo(ExampleWorkflow.Lab27RequestAcknowledge,
                "LAB-27 - Request acknowledge", "RSP^K11", "LIS to Instrument"),
            new ExampleWorkflowInfo(ExampleWorkflow.Lab28TestOrderSubmission,
                "LAB-28 - Test order submission", "OML^O33", "LIS to Instrument"),
            new ExampleWorkflowInfo(ExampleWorkflow.Lab28TestOrderResponse,
                "LAB-28 - Response to a test order", "ORL^O34", "Instrument to LIS"),
            new ExampleWorkflowInfo(ExampleWorkflow.Lab29TestResult,
                "LAB-29 - Test result", "OUL^R22", "Instrument to LIS"),
            new ExampleWorkflowInfo(ExampleWorkflow.Lab29ResultAccepted,
                "LAB-29 - Result accepted", "ACK^R22", "LIS to Instrument"),
            new ExampleWorkflowInfo(ExampleWorkflow.ControlTestResult,
                "Control - Test result", "OUL^R22", "Instrument to LIS"),
        };

        /// <summary>Looks up the metadata for a workflow.</summary>
        public static ExampleWorkflowInfo For(ExampleWorkflow workflow)
        {
            foreach (var info in All)
            {
                if (info.Workflow == workflow)
                {
                    return info;
                }
            }

            // Enum is closed; this is unreachable in practice.
            return new ExampleWorkflowInfo(workflow, workflow.ToString(), string.Empty, string.Empty);
        }
    }
}
