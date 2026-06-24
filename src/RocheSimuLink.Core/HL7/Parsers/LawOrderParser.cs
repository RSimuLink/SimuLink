using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// Parses x800DM IHE-LAW order-flow messages back into their models:
    /// OML^O33 test order submissions and ORL^O34 order responses. Symmetric to
    /// <see cref="Law.LawOmlO33Builder"/> / <see cref="Law.LawOrlO34Builder"/>.
    /// </summary>
    public static class LawOrderParser
    {
        public static TestOrderMessage ParseOrder(string rawMessage) =>
            ParseOrder(Hl7Parser.Parse(rawMessage));

        public static TestOrderMessage ParseOrder(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var order = new TestOrderMessage
            {
                Header = LawSegmentReader.ReadHeader(message),
                Specimen = ParseSpecimen(message),
            };

            // OML groups are ORC then (optionally) OBR + TCD. Walk segments in
            // order, opening a new TestOrder on each ORC.
            TestOrder? current = null;
            foreach (var segment in message.Segments)
            {
                switch (segment.Name)
                {
                    case "ORC":
                        current = new TestOrder
                        {
                            OrderControl = segment.Field(1),
                            TransactionDateTime = segment.Field(9),
                        };
                        order.Orders.Add(current);
                        break;

                    case "OBR" when current is not null:
                        current.PlacerOrderNumber = segment.Field(2);
                        current.TestCode = LawSegmentReader.ReadCoded(segment, 4);
                        break;

                    case "TCD" when current is not null:
                        current.ConsumptionVolume = segment.Field(9);
                        break;
                }
            }

            return order;
        }

        public static TestOrderResponse ParseResponse(string rawMessage) =>
            ParseResponse(Hl7Parser.Parse(rawMessage));

        public static TestOrderResponse ParseResponse(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var response = new TestOrderResponse
            {
                Header = LawSegmentReader.ReadHeader(message),
            };

            var msa = message.Segment("MSA");
            if (msa is not null)
            {
                response.AcknowledgmentCode = msa.Field(1);
                response.AcknowledgedControlId = msa.Field(2);
            }

            // The "unavailable" variant has no SPM; signal that with a null specimen.
            response.Specimen = message.Segment("SPM") is not null
                ? ParseSpecimen(message)
                : null;

            foreach (var orc in message.AllSegments("ORC"))
            {
                response.Orders.Add(new TestOrderResponseItem
                {
                    OrderControl = orc.Field(1),
                    PlacerOrderNumber = orc.Field(2),
                    OrderStatus = orc.Field(5),
                });
            }

            return response;
        }

        private static Specimen ParseSpecimen(ParsedHl7Message message)
        {
            var specimen = new Specimen();

            var spm = message.Segment("SPM");
            if (spm is not null)
            {
                // SPM-2 is "sampleId&namespace".
                var entity = spm.Field(2);
                if (entity.Length > 0)
                {
                    var parts = entity.Split('&');
                    specimen.SampleId = parts[0];
                    specimen.Namespace = parts.Length > 1 ? parts[1] : string.Empty;
                }
                else
                {
                    specimen.SuppressSpmIdentity = true;
                }

                specimen.SpecimenType = LawSegmentReader.ReadCoded(spm, 4);
                specimen.Role = spm.Component(11, 1);
            }

            var sac = message.Segment("SAC");
            if (sac is not null)
            {
                if (specimen.SampleId.Length == 0)
                {
                    specimen.SampleId = sac.Field(3);
                }

                specimen.CarrierId = sac.Field(10);
                specimen.CarrierPosition = sac.Field(11);
            }

            return specimen;
        }
    }
}
