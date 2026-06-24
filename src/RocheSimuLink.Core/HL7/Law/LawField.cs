using System.Text;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Helper for assembling HL7 segments by 1-based field index, then rendering
    /// with trailing empty fields trimmed (matching the x800DM trace style).
    /// </summary>
    internal sealed class LawField
    {
        private readonly string _segmentName;
        private readonly Dictionary<int, string> _fields = new();

        public LawField(string segmentName) => _segmentName = segmentName;

        /// <summary>Sets the 1-based field (HL7 numbering after the segment name).</summary>
        public LawField Set(int index, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _fields[index] = value;
            }

            return this;
        }

        /// <summary>
        /// Renders the segment, dropping trailing empty fields. For MSH the
        /// field separator occupies position 1 and encoding chars position 2.
        /// </summary>
        public string Render()
        {
            var max = _fields.Count == 0 ? 0 : _fields.Keys.Max();
            var sb = new StringBuilder(_segmentName);

            var isMsh = _segmentName == "MSH";
            // For MSH the separator after "MSH" *is* MSH-1, so MSH-2 (encoding
            // chars) is the first value emitted. Skipping i==1 makes the first
            // appended pipe stand in for MSH-1.
            for (var i = 1; i <= max; i++)
            {
                if (isMsh && i == 1)
                {
                    continue;
                }

                sb.Append('|');
                if (_fields.TryGetValue(i, out var value))
                {
                    sb.Append(value);
                }
            }

            return sb.ToString();
        }
    }
}
