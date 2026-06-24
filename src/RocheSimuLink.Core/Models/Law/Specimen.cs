namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// A specimen in the IHE-LAW workflow. Identity is the sample ID with a
    /// namespace (SPM-2 entity identifier, e.g. "$0E0EYXDR&amp;ROCHE").
    /// </summary>
    public sealed class Specimen
    {
        /// <summary>Sample identifier without namespace (e.g. "$0E0EYXDR").</summary>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>Assigning namespace for the sample id (e.g. "ROCHE").</summary>
        public string Namespace { get; set; } = "ROCHE";

        /// <summary>Specimen type, SPM-4 (e.g. PLAS^plasma^HL70487).</summary>
        public CodedElement SpecimenType { get; set; } = new("PLAS", "plasma", "HL70487");

        /// <summary>Specimen role, SPM-11 (e.g. "P" patient, "Q" QC).</summary>
        public string Role { get; set; } = "P";

        /// <summary>Container carrier id, SAC-10 (e.g. "1692").</summary>
        public string CarrierId { get; set; } = string.Empty;

        /// <summary>Position on the carrier, SAC-11 (e.g. "2").</summary>
        public string CarrierPosition { get; set; } = string.Empty;

        /// <summary>
        /// When true, SPM-2 is left empty even though the sample id is known.
        /// Used by the OML^O33 "no order available" response, where SPM-2 is
        /// blank (role "U") but SAC-3 still carries the sample id.
        /// </summary>
        public bool SuppressSpmIdentity { get; set; }

        /// <summary>SPM-2 rendered as "sampleId&amp;namespace".</summary>
        public string EntityIdentifier
        {
            get
            {
                if (SuppressSpmIdentity)
                {
                    return string.Empty;
                }

                return Namespace.Length > 0 ? $"{SampleId}&{Namespace}" : SampleId;
            }
        }
    }
}
