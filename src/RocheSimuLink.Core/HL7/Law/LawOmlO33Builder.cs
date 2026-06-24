using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds an x800DM IHE-LAW Test Order submission (OML^O33).
    ///
    /// Segment order per the Host Interface Manual / observed trace:
    ///   MSH, SPM, SAC, (per order: ORC, OBR, TCD)
    ///
    /// A "no order available" message omits the ORDER groups; the caller
    /// supplies a specimen whose role is "U" and whose SPM-2 is empty.
    /// </summary>
    public static class LawOmlO33Builder
    {
        private const string MessageType = "OML^O33^OML_O33";
        private const string DefaultProfile = "LAB-28^IHE";

        public static Hl7Message Build(TestOrderMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var header = message.Header;
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
                LawSpecimenBuilder.BuildSpm(message.Specimen),
                LawSpecimenBuilder.BuildSac(message.Specimen),
            };

            foreach (var order in message.Orders)
            {
                segments.Add(BuildOrc(order));

                // The no-order/discontinue (DC) reply carries ORC only.
                if (order.OrderControl == "DC")
                {
                    continue;
                }

                segments.Add(BuildObr(order));
                if (!string.IsNullOrEmpty(order.ConsumptionVolume))
                {
                    segments.Add(BuildTcd(order));
                }
            }

            return new Hl7Message
            {
                MessageType = "OML^O33",
                RawMessage = string.Join("\r", segments),
            };
        }

        private static string BuildOrc(TestOrder o) => new LawField("ORC")
            .Set(1, o.OrderControl)
            .Set(9, o.TransactionDateTime)
            .Render();

        private static string BuildObr(TestOrder o) => new LawField("OBR")
            .Set(2, o.PlacerOrderNumber)
            .Set(4, o.TestCode.ToHl7())
            .Render();

        private static string BuildTcd(TestOrder o) => new LawField("TCD")
            .Set(1, o.TestCode.ToHl7())
            .Set(9, o.ConsumptionVolume)
            .Render();
    }
}
