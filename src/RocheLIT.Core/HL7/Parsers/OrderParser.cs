using RocheLIT.Models.Orders;

namespace RocheLIT.HL7.Parsers
{
    /// <summary>
    /// Projects a parsed HL7 order message (OML/ORM/QBP-style) into the
    /// <see cref="ReceivedOrder"/> shape shown in the UI's order panel.
    /// </summary>
    public static class OrderParser
    {
        public static ReceivedOrder Parse(string rawMessage)
        {
            var parsed = Hl7Parser.Parse(rawMessage);
            return ToOrder(parsed);
        }

        public static ReceivedOrder ToOrder(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var order = new ReceivedOrder
            {
                MessageType = message.MessageType,
            };

            // Order number: prefer ORC-2 (placer), fall back to ORC-3 (filler).
            var orc = message.Segment("ORC");
            if (orc is not null)
            {
                order.OrderNumber = Coalesce(orc.Field(2), orc.Field(3));
            }

            // Sample id: prefer SPM-2, fall back to first OBR-3.
            var spm = message.Segment("SPM");
            var firstObr = message.Segment("OBR");
            order.SampleId = Coalesce(spm?.Field(2) ?? string.Empty, firstObr?.Field(3) ?? string.Empty);
            order.SampleType = spm?.Field(4) ?? string.Empty;

            var defaultPriority = orc is not null ? NormalizePriority(orc.Field(7)) : string.Empty;

            foreach (var obr in message.AllSegments("OBR"))
            {
                order.Tests.Add(new OrderedTest
                {
                    TestCode = obr.Component(4, 1),
                    TestName = obr.Component(4, 2),
                    Priority = Coalesce(NormalizePriority(obr.Field(27)), defaultPriority),
                });
            }

            order.TestType = JoinDistinct(order.Tests
                .Select(t => Coalesce(t.TestName, t.TestCode)));
            order.SampleVolume = JoinDistinct(message.AllSegments("TCD")
                .Select(tcd => tcd.Field(9)));

            return order;
        }

        private static string NormalizePriority(string code) => code.Trim().ToUpperInvariant() switch
        {
            "S" or "STAT" => "STAT",
            "R" or "ROUTINE" => "Routine",
            "A" or "ASAP" => "ASAP",
            "" => string.Empty,
            _ => code.Trim(),
        };

        private static string Coalesce(string primary, string fallback) =>
            primary.Length > 0 ? primary : fallback;

        private static string JoinDistinct(IEnumerable<string> values) =>
            string.Join(", ", values
                .Select(v => v.Trim())
                .Where(v => v.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }
}
