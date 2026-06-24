using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds an x800DM IHE-LAW Response to a test order submission (ORL^O34).
    ///
    /// Segment order per the Host Interface Manual / observed trace:
    ///   MSH, MSA, [SPM, SAC, (per order: ORC)]
    ///
    /// The "unavailable test order" variant carries only MSH + MSA (no specimen
    /// and no orders).
    /// </summary>
    public static class LawOrlO34Builder
    {
        private const string MessageType = "ORL^O34^ORL_O42";
        private const string DefaultProfile = "LAB-28^IHE";

        public static Hl7Message Build(TestOrderResponse response)
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
            };

            if (response.Specimen is not null)
            {
                segments.Add(LawSpecimenBuilder.BuildSpm(response.Specimen));
                segments.Add(LawSpecimenBuilder.BuildSac(response.Specimen));

                foreach (var order in response.Orders)
                {
                    segments.Add(BuildOrc(order));
                }
            }

            return new Hl7Message
            {
                MessageType = "ORL^O34",
                RawMessage = string.Join("\r", segments),
            };
        }

        private static string BuildMsa(TestOrderResponse r) => new LawField("MSA")
            .Set(1, r.AcknowledgmentCode)
            .Set(2, r.AcknowledgedControlId)
            .Render();

        private static string BuildOrc(TestOrderResponseItem o) => new LawField("ORC")
            .Set(1, o.OrderControl)
            .Set(2, o.PlacerOrderNumber)
            .Set(5, o.OrderStatus)
            .Render();
    }
}
