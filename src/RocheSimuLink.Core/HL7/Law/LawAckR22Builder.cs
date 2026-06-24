using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds an x800DM IHE-LAW Result Accepted acknowledgment (ACK^R22).
    ///
    /// Segment order per the Host Interface Manual:
    ///   MSH, MSA, [ERR*]  (ERR only when MSA-1 is not "AA")
    /// </summary>
    public static class LawAckR22Builder
    {
        private const string MessageType = "ACK^R22^ACK";
        private const string DefaultProfile = "LAB-29^IHE";

        public static Hl7Message Build(ResultAcknowledgment ack)
        {
            ArgumentNullException.ThrowIfNull(ack);

            var header = ack.Header;
            if (string.IsNullOrEmpty(header.MessageType))
            {
                header.MessageType = MessageType;
            }

            if (string.IsNullOrEmpty(header.ProfileIdentifier))
            {
                header.ProfileIdentifier = DefaultProfile;
            }

            // Acknowledgments leave MSH-15/16 empty (no further ack expected).

            var segments = new List<string>
            {
                LawMshBuilder.Build(header),
                BuildMsa(ack),
            };

            // ERR segments are prohibited unless the acknowledgment is negative.
            if (ack.AcknowledgmentCode != "AA")
            {
                foreach (var error in ack.Errors)
                {
                    segments.Add(BuildErr(error));
                }
            }

            return new Hl7Message
            {
                MessageType = "ACK^R22",
                RawMessage = string.Join("\r", segments),
            };
        }

        private static string BuildMsa(ResultAcknowledgment a) => new LawField("MSA")
            .Set(1, a.AcknowledgmentCode)
            .Set(2, a.AcknowledgedControlId)
            .Render();

        private static string BuildErr(AcknowledgmentError e) => new LawField("ERR")
            .Set(2, BuildErrorLocation(e))
            .Set(3, e.ErrorCode.ToHl7())
            .Set(4, e.Severity)
            .Set(8, e.UserMessage)
            .Render();

        private static string BuildErrorLocation(AcknowledgmentError e)
        {
            // ERR-2 is an ERL: segmentId^sequence^field, trailing empties trimmed.
            var components = new[] { e.SegmentId, e.SegmentSequence, e.FieldNumber };
            var lastNonEmpty = -1;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].Length > 0)
                {
                    lastNonEmpty = i;
                }
            }

            if (lastNonEmpty < 0)
            {
                return string.Empty;
            }

            return string.Join('^', components[..(lastNonEmpty + 1)]);
        }
    }
}
