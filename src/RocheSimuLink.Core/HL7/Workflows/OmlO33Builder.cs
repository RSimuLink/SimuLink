using RocheSimuLink.HL7.Builders;
using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Workflows
{
    public static class OmlO33Builder
    {
        public static Hl7Message Build(
            string sampleId,
            SampleType sampleType,
            TestType test)
        {
            var msh = MshBuilder.Build("OML^O33");
            var pid = PidBuilder.Build(sampleId);
            var spm = SpmBuilder.Build(sampleId, sampleType.Hl7Code);
            var obr = ObrBuilder.Build(sampleId, test.UniversalServiceIdentifier);

            string msg = string.Join("\r", msh, pid, spm, obr);

            return new Hl7Message
            {
                MessageType = "OML^O33",
                RawMessage = msg
            };
        }
    }
}
