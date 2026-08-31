namespace RocheLIT.HL7.Validation
{
    /// <summary>A precise validation problem found in an inbound HL7 message.</summary>
    public sealed class Hl7ValidationIssue
    {
        public Hl7ValidationIssue(string segmentName, int segmentOccurrence, int fieldPosition, string message)
        {
            SegmentName = segmentName;
            SegmentOccurrence = segmentOccurrence;
            FieldPosition = fieldPosition;
            Message = message;
        }

        public string SegmentName { get; }
        public int SegmentOccurrence { get; }
        public int FieldPosition { get; }
        public string Message { get; }

        public string Location =>
            FieldPosition > 0
                ? $"{SegmentName}[{SegmentOccurrence}]-{FieldPosition}"
                : $"{SegmentName}[{SegmentOccurrence}]";

        public override string ToString() => $"{Location}: {Message}";
    }
}
