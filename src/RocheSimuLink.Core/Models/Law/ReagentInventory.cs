namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// A consumable/reagent reported in an INV segment of a LAW result.
    /// </summary>
    public sealed class ReagentInventory
    {
        /// <summary>INV-1 substance identifier (e.g. "Reagent cassette^^99ROC").</summary>
        public CodedElement SubstanceId { get; set; } = new();

        /// <summary>INV-2 substance status (e.g. OK^^HL70383).</summary>
        public CodedElement Status { get; set; } = new("OK", "", "HL70383");

        /// <summary>INV-3 substance type (e.g. MR^^HL70384 reagent, CO consumable).</summary>
        public CodedElement SubstanceType { get; set; } = new("MR", "", "HL70384");

        /// <summary>INV-12 expiry date/time (e.g. 20291231225959+0100).</summary>
        public string ExpiryDateTime { get; set; } = string.Empty;

        /// <summary>INV-16 lot number (e.g. "HHHHIV").</summary>
        public string LotNumber { get; set; } = string.Empty;
    }
}
