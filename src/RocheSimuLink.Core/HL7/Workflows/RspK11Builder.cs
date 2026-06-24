using RocheSimuLink.HL7.Builders;
using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Workflows
{
    public static class RspK11Builder
    {
        public static Hl7Message Build(
            string sampleId,
            SampleType sampleType,
            TestType test)
        {
            var msh = MshBuilder.Build("RSP^K11");
            var msa = $"MSA|AA|{sampleId}";
            var qak = "QAK|OK";
            var qpd = $"QPD|LAB-27|{sampleId}";
            var pid = PidBuilder.Build(sampleId);
            var spm = SpmBuilder.Build(sampleId, sampleType.Hl7Code);
            var obr = ObrBuilder.Build(sampleId, test.UniversalServiceIdentifier);

            string msg = string.Join("\r", msh, msa, qak, qpd, pid, spm, obr);

            return new Hl7Message
            {
                MessageType = "RSP^K11",
                RawMessage = msg
            };
        }
    }
}
