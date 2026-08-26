namespace RocheLIT.Models.Workflows
{
    /// <summary>
    /// The values typed into the main UI that drive example generation: the same
    /// fields used when sending a real result (Sample ID, Test Type, Result,
    /// Sample Type, Sample Volume). No LIS connection is
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

        /// <summary>Rack ID mapped to SAC-10. Defaults to 0000 in generated examples when blank.</summary>
        public string RackId { get; set; } = string.Empty;

        /// <summary>Position in carrier mapped to SAC-11. Defaults to 0 in generated examples when blank.</summary>
        public string CarrierPosition { get; set; } = string.Empty;

        /// <summary>When true, generated LAB-29 examples include the seven INV segments.</summary>
        public bool IncludeInventory { get; set; }

        /// <summary>When true, generated LAB-29 examples include supplemental CT values.</summary>
        public bool IncludeCtValues { get; set; }

        /// <summary>The result value (OBX-5).</summary>
        public string ResultValue { get; set; } = string.Empty;

        /// <summary>The abnormal flag (OBX-8).</summary>
        public ResultFlag ResultFlag { get; set; } = ResultFlag.Normal;
    }
}
