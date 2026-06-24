namespace RocheSimuLink.Models.Orders
{
    /// <summary>
    /// Patient demographics parsed from a PID segment.
    /// </summary>
    public sealed class Patient
    {
        /// <summary>Patient identifier (PID-3).</summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>Display name "Given Family" (from PID-5 components).</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Date of birth (PID-7), null when absent or unparseable.</summary>
        public DateOnly? DateOfBirth { get; set; }

        /// <summary>Administrative sex (PID-8): e.g. "M", "F", "O", "U".</summary>
        public string Sex { get; set; } = string.Empty;
    }
}
