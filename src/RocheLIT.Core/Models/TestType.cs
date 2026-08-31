namespace RocheLIT.Models
{
    /// <summary>
    /// A test/assay the instrument can run, as defined by the host interface manual.
    /// </summary>
    public class TestType
    {
        /// <summary>Human-readable test name shown in the UI.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>OBR-4 Universal Service Identifier (assay code).</summary>
        public string UniversalServiceIdentifier { get; set; } = string.Empty;

        /// <summary>Targets/analytes this test produces results for.</summary>
        public List<Target> Targets { get; set; } = new();

        /// <summary>
        /// Sample volumes allowed for this test. Empty means all configured volumes apply.
        /// </summary>
        public List<SampleVolume> AllowedVolumes { get; set; } = new();

        /// <summary>
        /// Sample types allowed for this test/assay. Empty means all configured
        /// sample types apply.
        /// </summary>
        public List<SampleType> AllowedSampleTypes { get; set; } = new();

        /// <summary>
        /// Control result options listed for this assay in the host interface
        /// manual. These become the control INV-1 values in QC OUL^R22 messages.
        /// </summary>
        public List<ControlResult> ControlResults { get; set; } = new();
    }
}
