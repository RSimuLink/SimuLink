using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds an x800DM IHE-LAW Test Result (OUL^R22) message.
    ///
    /// Segment order per the Host Interface Manual / observed trace:
    ///   MSH, SPM, SAC, (per test: OBR, ORC, TCD, OBX*, INV*)
    /// </summary>
    public static class LawOulR22Builder
    {
        private const string MessageType = "OUL^R22^OUL_R22";

        public static Hl7Message Build(LawResultMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var segments = new List<string>
            {
                BuildMsh(message),
                LawSpecimenBuilder.BuildSpm(message.Specimen),
                LawSpecimenBuilder.BuildSac(message.Specimen),
            };

            foreach (var test in message.Tests)
            {
                segments.Add(BuildObr(test));
                segments.Add(BuildOrc(test));
                if (!string.IsNullOrEmpty(test.ConsumptionVolume))
                {
                    segments.Add(BuildTcd(test));
                }

                foreach (var obx in test.Observations)
                {
                    segments.Add(BuildObx(obx));
                }

                foreach (var inv in test.Reagents)
                {
                    segments.Add(BuildInv(inv));
                }
            }

            return new Hl7Message
            {
                MessageType = "OUL^R22",
                RawMessage = string.Join("\r", segments),
            };
        }

        private static string BuildMsh(LawResultMessage m) => new LawField("MSH")
            .Set(2, "^~\\&")
            .Set(3, m.SendingApplication)
            .Set(5, m.ReceivingApplication)
            .Set(7, m.MessageDateTime)
            .Set(9, MessageType)
            .Set(10, m.MessageControlId)
            .Set(11, "P")
            .Set(12, "2.5.1")
            .Set(15, "NE")
            .Set(16, "AL")
            .Set(18, "UNICODE UTF-8")
            .Set(21, "LAB-29^IHE")
            .Render();

        private static string BuildObr(LawTestResult t) => new LawField("OBR")
            .Set(2, t.SetId)
            .Set(4, t.TestCode.ToHl7())
            .Render();

        private static string BuildOrc(LawTestResult t) => new LawField("ORC")
            .Set(1, t.OrderControl)
            .Set(2, t.SetId)
            .Set(5, t.OrderStatus)
            .Render();

        private static string BuildTcd(LawTestResult t) => new LawField("TCD")
            .Set(1, t.TestCode.ToHl7())
            .Set(9, t.ConsumptionVolume)
            .Render();

        private static string BuildObx(ChannelResult o) => new LawField("OBX")
            .Set(1, o.SetId)
            .Set(2, o.ValueType)
            .Set(3, o.ObservationId.ToHl7())
            .Set(4, o.SubId)
            .Set(5, o.Value)
            .Set(6, o.Units?.ToHl7() ?? string.Empty)
            .Set(8, o.Interpretation?.ToHl7() ?? string.Empty)
            .Set(11, o.ResultStatus)
            .Set(16, o.ObservationMethod)
            .Set(18, o.AnalysisDateTime)
            .Set(20, o.EquipmentInstanceId)
            .Set(29, o.ObservationType)
            .Render();

        private static string BuildInv(ReagentInventory r) => new LawField("INV")
            .Set(1, r.SubstanceId.ToHl7())
            .Set(2, r.Status.ToHl7())
            .Set(3, r.SubstanceType.ToHl7())
            .Set(12, r.ExpiryDateTime)
            .Set(16, r.LotNumber)
            .Render();
    }
}
