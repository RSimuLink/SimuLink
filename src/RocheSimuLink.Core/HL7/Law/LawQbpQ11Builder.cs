using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds an x800DM IHE-LAW Work Order Step query (QBP^Q11).
    ///
    /// Segment order per the Host Interface Manual / observed trace:
    ///   MSH, QPD, RCP
    /// </summary>
    public static class LawQbpQ11Builder
    {
        private const string MessageType = "QBP^Q11^QBP_Q11";
        private const string DefaultProfile = "LAB-27R^ROCHE";

        public static Hl7Message Build(WorkOrderQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var header = query.Header;
            if (string.IsNullOrEmpty(header.MessageType))
            {
                header.MessageType = MessageType;
            }

            if (string.IsNullOrEmpty(header.ProfileIdentifier))
            {
                header.ProfileIdentifier = DefaultProfile;
            }

            // Requests carry accept/application ack types (NE/AL).
            if (string.IsNullOrEmpty(header.AcceptAcknowledgment))
            {
                header.AcceptAcknowledgment = "NE";
            }

            if (string.IsNullOrEmpty(header.ApplicationAcknowledgment))
            {
                header.ApplicationAcknowledgment = "AL";
            }

            var segments = new List<string>
            {
                LawMshBuilder.Build(header),
                BuildQpd(query),
                BuildRcp(query),
            };

            return new Hl7Message
            {
                MessageType = "QBP^Q11",
                RawMessage = string.Join("\r", segments),
            };
        }

        private static string BuildQpd(WorkOrderQuery q) => new LawField("QPD")
            .Set(1, q.QueryName.ToHl7())
            .Set(2, q.QueryTag)
            .Set(3, q.SampleId)
            .Set(4, q.CarrierId)
            .Set(5, q.CarrierPosition)
            .Render();

        private static string BuildRcp(WorkOrderQuery q) => new LawField("RCP")
            .Set(1, q.QueryPriority)
            .Set(3, q.ResponseModality.ToHl7())
            .Render();
    }
}
