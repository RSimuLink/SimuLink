namespace RocheSimuLink.Models.Workflows
{
    /// <summary>
    /// The values typed into the main UI that drive example generation: the same
    /// fields used when sending a real result (Sample ID, Test Type, Result,
    /// Sample Type, Sample Volume, Result Status, Flag). No LIS connection is
    /// required — the generator only formats messages from these inputs.
    /// </summary>
    public sealed class ExampleGeneratorInput
    {
        /// <summary>Sample ID (barcode) from the main UI.</summary>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>The selected test/assay.</summary>
        public TestType Test { get; set; } = new();

        /// <summary>The selected target/analyte (defaults to the test's first).</summary>
        public Target? Target { get; set; }

        /// <summary>The selected sample type.</summary>
        public SampleType SampleType { get; set; } = new();

        /// <summary>The selected sample volume (e.g. "500 uL"); may be empty.</summary>
        public string SampleVolume { get; set; } = string.Empty;

        /// <summary>The result value (OBX-5).</summary>
        public string ResultValue { get; set; } = string.Empty;

        /// <summary>The result status (OBX-11).</summary>
        public ResultStatus ResultStatus { get; set; } = ResultStatus.Final;

        /// <summary>The abnormal flag (OBX-8).</summary>
        public ResultFlag ResultFlag { get; set; } = ResultFlag.Normal;
    }
}
