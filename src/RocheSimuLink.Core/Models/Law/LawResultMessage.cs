namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// The data needed to build an x800DM IHE-LAW OUL^R22 (test result upload):
    /// message header context, the specimen, and its test results.
    /// </summary>
    public sealed class LawResultMessage
    {
        /// <summary>MSH-3 sending application (e.g. "X800DM").</summary>
        public string SendingApplication { get; set; } = "X800DM";

        /// <summary>MSH-5 receiving application (e.g. "Host").</summary>
        public string ReceivingApplication { get; set; } = "Host";

        /// <summary>MSH-7 message date/time with offset (e.g. 20241030110039+0100).</summary>
        public string MessageDateTime { get; set; } = string.Empty;

        /// <summary>MSH-10 message control id (a GUID in the trace).</summary>
        public string MessageControlId { get; set; } = string.Empty;

        /// <summary>The specimen these results belong to.</summary>
        public Specimen Specimen { get; set; } = new();

        /// <summary>The test results carried by this message.</summary>
        public List<LawTestResult> Tests { get; set; } = new();
    }
}
