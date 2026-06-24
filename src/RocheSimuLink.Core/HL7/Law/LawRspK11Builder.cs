using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds an x800DM IHE-LAW Work Order Step query response (RSP^K11).
    ///
    /// Segment order per the Host Interface Manual / observed trace:
    ///   MSH, MSA, QAK, QPD
    /// </summary>
    public static class LawRspK11Builder
    {
        private const string MessageType = "RSP^K11^RSP_K11";
        private const string DefaultProfile = "LAB-27R^ROCHE";

        public static Hl7Message Build(WorkOrderQueryResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            var header = response.Header;
            if (string.IsNullOrEmpty(header.MessageType))
            {
                header.MessageType = MessageType;
            }

            if (string.IsNullOrEmpty(header.ProfileIdentifier))
            {
                header.ProfileIdentifier = DefaultProfile;
            }

            // Responses leave MSH-15/16 empty (no further ack expected).

            var segments = new List<string>
            {
                LawMshBuilder.Build(header),
                BuildMsa(response),
                BuildQak(response),
                BuildQpd(response),
            };

            return new Hl7Message
            {
                MessageType = "RSP^K11",
                RawMessage = string.Join("\r", segments),
            };
        }

        private static string BuildMsa(WorkOrderQueryResponse r) => new LawField("MSA")
            .Set(1, r.AcknowledgmentCode)
            .Set(2, r.AcknowledgedControlId)
            .Render();

        private static string BuildQak(WorkOrderQueryResponse r) => new LawField("QAK")
            .Set(1, r.QueryTag)
            .Set(2, r.QueryResponseStatus)
            .Set(3, r.QueryName.ToHl7())
            .Render();

        private static string BuildQpd(WorkOrderQueryResponse r) => new LawField("QPD")
            .Set(1, r.QueryName.ToHl7())
            .Set(2, r.EchoedQueryTag)
            .Set(3, r.SampleId)
            .Set(4, r.CarrierId)
            .Set(5, r.CarrierPosition)
            .Render();
    }
}
