using RocheSimuLink.HL7.Builders;
using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Workflows
{
    public static class OrlO34Builder
    {
        public static Hl7Message Build(
            string sampleId,
            SampleType sampleType,
            TestType test)
        {
            var msh = MshBuilder.Build("ORL^O34");
            var pid = PidBuilder.Build(sampleId);
            var spm = SpmBuilder.Build(sampleId, sampleType.Hl7Code);
            var orc = OrcBuilder.Build(sampleId);
            var obr = ObrBuilder.Build(sampleId, test.UniversalServiceIdentifier);

            string msg = string.Join("\r", msh, pid, spm, orc, obr);

            return new Hl7Message
            {
                MessageType = "ORL^O34",
                RawMessage = msg
            };
        }
    }
}
