namespace RocheSimuLink.Models.Him
{
    /// <summary>
    /// One sample type accepted by an assay, with its consumption volume, as
    /// listed in the assay's "Sample types and input volume" table
    /// (SPM-4 / TCD-9-1).
    /// </summary>
    public sealed class AssaySampleType
    {
        /// <summary>Display name from the table's "Name" column (e.g. "PLASMA").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>SPM-4 coded specimen type (e.g. "PLAS^plasma^HL70487").</summary>
        public string SpecimenType { get; set; } = string.Empty;

        /// <summary>TCD-9-1 input volume in microliters (e.g. "850").</summary>
        public string VolumeMicroliters { get; set; } = string.Empty;
    }

    /// <summary>
    /// A target/channel produced by an assay, with the result codes it can
    /// report, from the "Targets" and "Sample result codes" tables
    /// (OBX-3 / OBX-5 / OBX-8-1).
    /// </summary>
    public sealed class AssayTarget
    {
        /// <summary>Target name from the table (e.g. "CT").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>OBX-3 observation identifier (e.g. "CT^CT^99ROC").</summary>
        public string ObservationIdentifier { get; set; } = string.Empty;

        /// <summary>OBX-5 observation values this target can report (e.g. "Positive").</summary>
        public List<string> ObservationValues { get; set; } = new();

        /// <summary>OBX-8-1 interpretation codes paired with the values (e.g. "POS").</summary>
        public List<string> InterpretationCodes { get; set; } = new();
    }

    /// <summary>
    /// A test offered by an assay (OBR-4 / TCD-1). An assay may expose a panel
    /// test plus per-channel tests (e.g. CT/NG exposes "CT/NG", "CT", "NG").
    /// </summary>
    public sealed class AssayTest
    {
        /// <summary>Test name from the table (e.g. "CT/NG").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>OBR-4 / TCD-1 universal service identifier (e.g. "72828-7^CT/NG^LN").</summary>
        public string UniversalServiceIdentifier { get; set; } = string.Empty;
    }

    /// <summary>
    /// An assay (test family) as defined in the "Assays" chapter of the Host
    /// Interface Manual: the LIS mapping for its sample types, tests, targets,
    /// and result codes. This is the catalog the simulator uses to populate the
    /// UI and build OUL^R22 result messages.
    /// </summary>
    public sealed class AssayDefinition
    {
        /// <summary>Assay name as it appears in the "&lt;name&gt; - LIS mapping" heading (e.g. "CT/NG").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Free-text description line following the heading.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Whether the assay is quantitative (numeric) vs qualitative.</summary>
        public bool IsQuantitative { get; set; }

        /// <summary>Accepted sample types and their volumes.</summary>
        public List<AssaySampleType> SampleTypes { get; set; } = new();

        /// <summary>Tests offered (panel + per-channel).</summary>
        public List<AssayTest> Tests { get; set; } = new();

        /// <summary>Targets/channels and their result codes.</summary>
        public List<AssayTarget> Targets { get; set; } = new();
    }
}
