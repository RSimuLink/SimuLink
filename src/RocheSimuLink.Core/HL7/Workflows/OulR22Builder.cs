using RocheSimuLink.HL7.Builders;
using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models;

namespace RocheSimuLink.HL7.Workflows
{
    public static class OulR22Builder
    {
        /// <summary>
        /// Builds an OUL^R22 result with a free-text interpretation code and
        /// Final status.
        /// </summary>
        public static Hl7Message Build(
            string sampleId,
            SampleType sampleType,
            TestType test,
            Target target,
            string observationValue,
            string interpretationCode)
        {
            var obx = ObxBuilder.Build(target, observationValue, interpretationCode);
            return Assemble(sampleId, sampleType, test, obx);
        }

        /// <summary>
        /// Builds an OUL^R22 result using a typed abnormal flag (OBX-8) and a
        /// result status (OBX-11), matching the UI's Flags / Result Status inputs.
        /// </summary>
        public static Hl7Message Build(
            string sampleId,
            SampleType sampleType,
            TestType test,
            Target target,
            string observationValue,
            ResultFlag flag,
            ResultStatus status,
            ConnectionSettings? settings = null,
            string valueType = "ST")
        {
            var obx = ObxBuilder.Build(target, observationValue, flag.ToHl7Code(), status, setId: 1, valueType);
            return Assemble(sampleId, sampleType, test, obx, settings);
        }

        private static Hl7Message Assemble(
            string sampleId, SampleType sampleType, TestType test, string obx, ConnectionSettings? settings = null)
        {
            var msh = settings is null
                ? MshBuilder.Build("OUL^R22")
                : MshBuilder.Build("OUL^R22", settings);
            var pid = PidBuilder.Build(sampleId);
            var spm = SpmBuilder.Build(sampleId, sampleType.Hl7Code);
            var obr = ObrBuilder.Build(sampleId, test.UniversalServiceIdentifier);

            var msg = string.Join("\r", msh, pid, spm, obr, obx);

            return new Hl7Message
            {
                MessageType = "OUL^R22",
                RawMessage = msg,
            };
        }
    }
}
