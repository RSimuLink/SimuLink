namespace RocheLIT.Models
{
    /// <summary>
    /// A manual-defined control result option for an assay, used as INV-1 in
    /// QC OUL^R22 messages.
    /// </summary>
    public sealed class ControlResult
    {
        public string Name { get; set; } = string.Empty;

        public bool IsPositive { get; set; }

        public override string ToString() => Name;
    }
}
