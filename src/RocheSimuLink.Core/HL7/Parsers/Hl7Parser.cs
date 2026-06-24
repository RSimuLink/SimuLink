namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// Parses raw HL7 v2 text into a structured <see cref="ParsedHl7Message"/>.
    /// Segments may be separated by CR (standard), LF, or CRLF for tolerance.
    /// </summary>
    public static class Hl7Parser
    {
        private static readonly string[] SegmentSeparators = { "\r\n", "\r", "\n" };

        /// <exception cref="FormatException">
        /// Thrown when the message is empty or does not begin with an MSH segment.
        /// </exception>
        public static ParsedHl7Message Parse(string raw)
        {
            ArgumentNullException.ThrowIfNull(raw);

            var lines = raw
                .Split(SegmentSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => l.Length > 0)
                .ToArray();

            if (lines.Length == 0)
            {
                throw new FormatException("HL7 message is empty.");
            }

            if (!lines[0].StartsWith("MSH", StringComparison.Ordinal))
            {
                throw new FormatException("HL7 message must begin with an MSH segment.");
            }

            var encoding = Hl7Encoding.FromMshSegment(lines[0]);
            var segments = new List<Hl7Segment>(lines.Length);

            foreach (var line in lines)
            {
                segments.Add(ParseSegment(line, encoding));
            }

            return new ParsedHl7Message(segments, encoding);
        }

        private static Hl7Segment ParseSegment(string line, Hl7Encoding encoding)
        {
            var name = line.Length >= 3 ? line.Substring(0, 3) : line;

            // MSH is special: MSH-1 is the field separator itself and MSH-2 is
            // the encoding characters, so we reconstruct those two fields and
            // then split the remainder normally.
            if (name == "MSH")
            {
                // Layout: chars 0-2 = "MSH", char 3 = field separator (MSH-1),
                // chars 4-7 = encoding characters (MSH-2), char 8 = field
                // separator, char 9 onward = MSH-3 and beyond.
                var afterEncoding = line.Length > 9 ? line.Substring(9) : string.Empty;
                var rest = afterEncoding.Length > 0
                    ? afterEncoding.Split(encoding.FieldSeparator)
                    : Array.Empty<string>();

                var fields = new string[rest.Length + 2];
                fields[0] = encoding.FieldSeparator.ToString();        // MSH-1
                fields[1] = line.Length >= 8 ? line.Substring(4, 4) : string.Empty; // MSH-2
                Array.Copy(rest, 0, fields, 2, rest.Length);
                return new Hl7Segment(name, fields, encoding);
            }

            var parts = line.Split(encoding.FieldSeparator);
            // Drop the segment name; remaining entries are the fields.
            var segmentFields = parts.Skip(1).ToArray();
            return new Hl7Segment(name, segmentFields, encoding);
        }
    }
}
