namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// One ordered test and its results within a LAW OUL^R22 message: an OBR
    /// (LOINC test), its OBX observations, a TCD (consumption), and the INV
    /// reagent list used to produce it.
    /// </summary>
    public sealed class LawTestResult
    {
        /// <summary>OBR-2 set id (e.g. "1").</summary>
        public string SetId { get; set; } = "1";

        /// <summary>OBR-4 universal service identifier, LOINC (e.g. 70241-5^HIV^LN).</summary>
        public CodedElement TestCode { get; set; } = new();

        /// <summary>ORC-1 order control (e.g. "SC" status changed).</summary>
        public string OrderControl { get; set; } = "SC";

        /// <summary>ORC-5 order status (e.g. "CM" completed).</summary>
        public string OrderStatus { get; set; } = "CM";

        /// <summary>The OBX observations for this test.</summary>
        public List<ChannelResult> Observations { get; set; } = new();

        /// <summary>TCD-9 test consumption volume, UCUM (e.g. "500^uL&amp;&amp;UCUM").</summary>
        public string ConsumptionVolume { get; set; } = string.Empty;

        /// <summary>Reagents/consumables (INV segments) used for this test.</summary>
        public List<ReagentInventory> Reagents { get; set; } = new();
    }
}
