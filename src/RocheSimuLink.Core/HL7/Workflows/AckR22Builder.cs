using RocheSimuLink.HL7.Builders;
using RocheSimuLink.HL7.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Workflows
{
    public static class AckR22Builder
    {
        public static Hl7Message Build(string sampleId)
        {
            var msh = MshBuilder.Build("ACK^R22");
            var msa = $"MSA|AA|{sampleId}"; // AA = Application Accept

            string msg = string.Join("\r", msh, msa);

            return new Hl7Message
            {
                MessageType = "ACK^R22",
                RawMessage = msg
            };
        }
    }
}
