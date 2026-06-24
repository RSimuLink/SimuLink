using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// Parses x800DM IHE-LAW Result Accepted acknowledgments (ACK^R22) back into
    /// the <see cref="ResultAcknowledgment"/> model. Symmetric to
    /// <see cref="Law.LawAckR22Builder"/>.
    /// </summary>
    public static class LawAckParser
    {
        public static ResultAcknowledgment Parse(string rawMessage) =>
            Parse(Hl7Parser.Parse(rawMessage));

        public static ResultAcknowledgment Parse(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var ack = new ResultAcknowledgment
            {
                Header = LawSegmentReader.ReadHeader(message),
            };

            var msa = message.Segment("MSA");
            if (msa is not null)
            {
                ack.AcknowledgmentCode = msa.Field(1);
                ack.AcknowledgedControlId = msa.Field(2);
            }

            foreach (var err in message.AllSegments("ERR"))
            {
                ack.Errors.Add(new AcknowledgmentError
                {
                    SegmentId = err.Component(2, 1),
                    SegmentSequence = err.Component(2, 2),
                    FieldNumber = err.Component(2, 3),
                    ErrorCode = LawSegmentReader.ReadCoded(err, 3),
                    Severity = err.Field(4),
                    UserMessage = err.Field(8),
                });
            }

            return ack;
        }
    }
}
