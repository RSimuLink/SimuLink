namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// A test order submission (OML^O33): the host's reply to a work order query
    /// telling the instrument which tests to run on a sample. Carries the
    /// specimen (SPM/SAC) and one or more ORDER groups.
    ///
    /// A "no order available" response omits the orders and carries a specimen
    /// with role "U" (unknown), per the Host Interface Manual.
    /// </summary>
    public sealed class TestOrderMessage
    {
        /// <summary>The MSH context for the order message.</summary>
        public LawMessageHeader Header { get; set; } = new();

        /// <summary>The specimen the order(s) apply to.</summary>
        public Specimen Specimen { get; set; } = new();

        /// <summary>The ordered tests (empty for a no-order response).</summary>
        public List<TestOrder> Orders { get; set; } = new();
    }
}
