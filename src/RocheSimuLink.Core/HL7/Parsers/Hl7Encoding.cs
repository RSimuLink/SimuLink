namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// HL7 delimiter set. Defaults match the standard encoding characters
    /// declared in MSH-1 / MSH-2: <c>|^~\&amp;</c>.
    /// </summary>
    public sealed class Hl7Encoding
    {
        public char FieldSeparator { get; init; } = '|';
        public char ComponentSeparator { get; init; } = '^';
        public char RepetitionSeparator { get; init; } = '~';
        public char EscapeCharacter { get; init; } = '\\';
        public char SubcomponentSeparator { get; init; } = '&';

        public static Hl7Encoding Default { get; } = new();

        /// <summary>
        /// Derives the delimiter set from an MSH segment line
        /// (e.g. <c>MSH|^~\&amp;|...</c>). Falls back to defaults when the
        /// line is too short to declare them.
        /// </summary>
        public static Hl7Encoding FromMshSegment(string mshSegment)
        {
            ArgumentNullException.ThrowIfNull(mshSegment);

            if (mshSegment.Length < 8 || !mshSegment.StartsWith("MSH", StringComparison.Ordinal))
            {
                return Default;
            }

            var field = mshSegment[3];
            var encodingChars = mshSegment.Substring(4, 4);

            return new Hl7Encoding
            {
                FieldSeparator = field,
                ComponentSeparator = encodingChars[0],
                RepetitionSeparator = encodingChars[1],
                EscapeCharacter = encodingChars[2],
                SubcomponentSeparator = encodingChars[3],
            };
        }
    }
}
