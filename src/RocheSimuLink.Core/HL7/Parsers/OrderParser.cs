using System.Globalization;
using RocheSimuLink.Models.Orders;

namespace RocheSimuLink.HL7.Parsers
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
            var order = ToOrder(parsed);
            order.RawMessage = rawMessage;
            return order;
        }

        public static ReceivedOrder ToOrder(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var order = new ReceivedOrder
            {
                MessageType = message.MessageType,
                Patient = ParsePatient(message.Segment("PID")),
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

            return order;
        }

        private static Patient ParsePatient(Hl7Segment? pid)
        {
            if (pid is null)
            {
                return new Patient();
            }

            return new Patient
            {
                PatientId = pid.Component(3, 1),
                FullName = FormatName(pid),
                DateOfBirth = ParseDate(pid.Field(7)),
                Sex = pid.Field(8),
            };
        }

        private static string FormatName(Hl7Segment pid)
        {
            // PID-5: Family^Given^Middle...
            var family = pid.Component(5, 1);
            var given = pid.Component(5, 2);

            if (family.Length == 0 && given.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(' ', new[] { given, family }.Where(p => p.Length > 0));
        }

        private static DateOnly? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // HL7 dates are typically yyyyMMdd, optionally with a time component.
            var datePart = value.Length >= 8 ? value[..8] : value;
            if (DateOnly.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            return null;
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
    }
}
