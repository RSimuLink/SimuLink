namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// One ORDER group within a test order submission (OML^O33): an ORC common
    /// order plus its OBR observation request and TCD test-code detail.
    /// </summary>
    public sealed class TestOrder
    {
        /// <summary>
        /// ORC-1 order control: "NW" new order, "CA" cancel, "DC" discontinue
        /// (used by the negative/no-order response).
        /// </summary>
        public string OrderControl { get; set; } = "NW";

        /// <summary>ORC-9 transaction date/time (yyyyMMddHHmmss).</summary>
        public string TransactionDateTime { get; set; } = string.Empty;

        /// <summary>OBR-2 placer order number (the host's unique order id).</summary>
        public string PlacerOrderNumber { get; set; } = string.Empty;

        /// <summary>OBR-4 / TCD-1 universal service identifier, LOINC (e.g. 72828-7^CT/NG^LN).</summary>
        public CodedElement TestCode { get; set; } = new();

        /// <summary>TCD-9 test consumption volume, UCUM (e.g. "850^uL&amp;&amp;UCUM").</summary>
        public string ConsumptionVolume { get; set; } = string.Empty;
    }
}
