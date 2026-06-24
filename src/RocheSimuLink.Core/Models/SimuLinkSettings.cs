using RocheSimuLink.Models.Workflows;

namespace RocheSimuLink.Models
{
    /// <summary>
    /// Root configuration for the simulator, derived from the host interface manual.
    /// </summary>
    public class SimuLinkSettings
    {
        /// <summary>Tests/assays the instrument can run.</summary>
        public List<TestType> TestTypes { get; set; } = new();

        /// <summary>Specimen types supported by the instrument.</summary>
        public List<SampleType> SampleTypes { get; set; } = new();

        /// <summary>Sample volume options.</summary>
        public List<SampleVolume> SampleVolumes { get; set; } = new();

        /// <summary>HL7 workflows the instrument supports.</summary>
        public List<Workflow> Workflows { get; set; } = new();

        /// <summary>LIS connection and HL7 identity settings.</summary>
        public ConnectionSettings Connection { get; set; } = new();
    }
}
