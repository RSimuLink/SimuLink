using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// Shared projection helpers for the IHE-LAW parsers: reading the MSH
    /// context and coded elements out of parsed segments.
    /// </summary>
    internal static class LawSegmentReader
    {
        public static LawMessageHeader ReadHeader(ParsedHl7Message message)
        {
            var msh = message.Segment("MSH");
            if (msh is null)
            {
                return new LawMessageHeader();
            }

            return new LawMessageHeader
            {
                SendingApplication = msh.Field(3),
                ReceivingApplication = msh.Field(5),
                MessageDateTime = msh.Field(7),
                MessageType = msh.Field(9),
                MessageControlId = msh.Field(10),
                ProcessingId = msh.Field(11),
                Version = msh.Field(12),
                AcceptAcknowledgment = msh.Field(15),
                ApplicationAcknowledgment = msh.Field(16),
                CharacterSet = msh.Field(18),
                ProfileIdentifier = msh.Field(21),
            };
        }

        public static CodedElement ReadCoded(Hl7Segment segment, int field) => new()
        {
            Identifier = segment.Component(field, 1),
            Text = segment.Component(field, 2),
            CodingSystem = segment.Component(field, 3),
        };
    }
}
