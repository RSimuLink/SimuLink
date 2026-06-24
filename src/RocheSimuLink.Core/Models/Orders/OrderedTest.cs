namespace RocheSimuLink.Models.Orders
{
    /// <summary>
    /// A single test requested in an inbound order. Maps to the right-panel grid
    /// columns: Test Code, Test Name, Priority.
    /// </summary>
    public sealed class OrderedTest
    {
        /// <summary>Test/assay code (OBR-4 component 1).</summary>
        public string TestCode { get; set; } = string.Empty;

        /// <summary>Human-readable test name (OBR-4 component 2).</summary>
        public string TestName { get; set; } = string.Empty;

        /// <summary>Priority such as "Routine" or "STAT" (OBR-27 / ORC-7).</summary>
        public string Priority { get; set; } = string.Empty;
    }
}
