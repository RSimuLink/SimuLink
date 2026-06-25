namespace RocheSimuLink.Models.Workflows
{
    /// <summary>
    /// The HL7 message flows the Example Generator can produce, matching the
    /// LAB-27/28/29 workflows in the "HIV HL7 Example" reference. Each value is
    /// one message in a direction; together they cover the full query → order →
    /// result → ack conversation plus the QC control result.
    /// </summary>
    public enum ExampleWorkflow
    {
        /// <summary>LAB-27 QBP^Q11 — work order request (Instrument → LIS).</summary>
        Lab27WorkOrderRequest,

        /// <summary>LAB-27 RSP^K11 — request acknowledge (LIS → Instrument).</summary>
        Lab27RequestAcknowledge,

        /// <summary>LAB-28 OML^O33 — test order submission (LIS → Instrument).</summary>
        Lab28TestOrderSubmission,

        /// <summary>LAB-28 ORL^O34 — response to a test order (Instrument → LIS).</summary>
        Lab28TestOrderResponse,

        /// <summary>LAB-29 OUL^R22 — test result (Instrument → LIS).</summary>
        Lab29TestResult,

        /// <summary>LAB-29 ACK^R22 — result accepted (LIS → Instrument).</summary>
        Lab29ResultAccepted,

        /// <summary>Control OUL^R22 — QC test result (Instrument → LIS).</summary>
        ControlTestResult,
    }
}
