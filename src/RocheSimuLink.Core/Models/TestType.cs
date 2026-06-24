namespace RocheSimuLink.Models
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
    }
}
