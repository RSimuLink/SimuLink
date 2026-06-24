namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// A parsed HL7 segment: a name (e.g. "MSH", "OBR") plus its raw fields.
    /// Field access is 1-based to match HL7 conventions (Field(1) is the first
    /// field after the segment name). For MSH, Field(1) is the field separator
    /// and Field(2) is the encoding characters, per the standard.
    /// </summary>
    public sealed class Hl7Segment
    {
        private readonly string[] _fields;
        private readonly Hl7Encoding _encoding;

        public Hl7Segment(string name, string[] fields, Hl7Encoding encoding)
        {
            Name = name;
            _fields = fields;
            _encoding = encoding;
        }

        /// <summary>Three-character segment name (e.g. "OBX").</summary>
        public string Name { get; }

        /// <summary>Number of fields (excluding the segment name).</summary>
        public int FieldCount => _fields.Length;

        /// <summary>
        /// Returns the raw value of the given 1-based field, or empty string
        /// when the field is absent.
        /// </summary>
        public string Field(int index)
        {
            if (index < 1 || index > _fields.Length)
            {
                return string.Empty;
            }

            return _fields[index - 1];
        }

        /// <summary>
        /// Returns a 1-based component of a 1-based field
        /// (split on the component separator, default '^').
        /// </summary>
        public string Component(int field, int component)
        {
            var raw = Field(field);
            if (raw.Length == 0)
            {
                return string.Empty;
            }

            var parts = raw.Split(_encoding.ComponentSeparator);
            if (component < 1 || component > parts.Length)
            {
                return string.Empty;
            }

            return parts[component - 1];
        }
    }
}
