namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// A parsed HL7 message: an ordered list of segments plus convenience
    /// accessors for the message type and common lookups.
    /// </summary>
    public sealed class ParsedHl7Message
    {
        public ParsedHl7Message(IReadOnlyList<Hl7Segment> segments, Hl7Encoding encoding)
        {
            Segments = segments;
            Encoding = encoding;
        }

        /// <summary>All segments in message order.</summary>
        public IReadOnlyList<Hl7Segment> Segments { get; }

        /// <summary>The delimiter set used to parse this message.</summary>
        public Hl7Encoding Encoding { get; }

        /// <summary>
        /// Message type as declared in MSH-9 (e.g. "OML^O33"), or empty string
        /// when no MSH segment is present.
        /// </summary>
        public string MessageType
        {
            get
            {
                var msh = Segment("MSH");
                if (msh is null)
                {
                    return string.Empty;
                }

                var code = msh.Component(9, 1);
                var trigger = msh.Component(9, 2);
                return trigger.Length > 0 ? $"{code}^{trigger}" : code;
            }
        }

        /// <summary>Returns the first segment with the given name, or null.</summary>
        public Hl7Segment? Segment(string name) =>
            Segments.FirstOrDefault(s => s.Name == name);

        /// <summary>Returns all segments with the given name in message order.</summary>
        public IEnumerable<Hl7Segment> AllSegments(string name) =>
            Segments.Where(s => s.Name == name);
    }
}
