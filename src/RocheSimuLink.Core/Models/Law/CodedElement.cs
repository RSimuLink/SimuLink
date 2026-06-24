namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// An HL7 CWE/CE coded element: identifier ^ text ^ coding system
    /// (e.g. "70241-5^HIV^LN" or "PLAS^plasma^HL70487").
    /// </summary>
    public sealed class CodedElement
    {
        public CodedElement() { }

        public CodedElement(string identifier, string text = "", string codingSystem = "")
        {
            Identifier = identifier;
            Text = text;
            CodingSystem = codingSystem;
        }

        public string Identifier { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string CodingSystem { get; set; } = string.Empty;

        /// <summary>
        /// Parses an HL7 CWE/CE string ("id^text^system") into a coded element.
        /// Missing components default to empty. Returns an empty element for a
        /// null/blank input.
        /// </summary>
        public static CodedElement Parse(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new CodedElement();
            }

            var parts = value.Split('^');
            return new CodedElement
            {
                Identifier = parts.Length > 0 ? parts[0] : string.Empty,
                Text = parts.Length > 1 ? parts[1] : string.Empty,
                CodingSystem = parts.Length > 2 ? parts[2] : string.Empty,
            };
        }

        /// <summary>Renders as "id^text^system", trimming trailing empty components.</summary>
        public string ToHl7()
        {
            if (CodingSystem.Length > 0)
            {
                return $"{Identifier}^{Text}^{CodingSystem}";
            }

            return Text.Length > 0 ? $"{Identifier}^{Text}" : Identifier;
        }

        public override string ToString() => ToHl7();
    }
}
