using RocheSimuLink.HL7.Builders;
using RocheSimuLink.HL7.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Workflows
{
    public static class QbpQ11Builder
    {
        public static Hl7Message Build(string instrumentId)
        {
            var msh = MshBuilder.Build("QBP^Q11");
            var qpd = $"QPD|LAB-27|{instrumentId}";
            var rcp = "RCP|I"; // Immediate response

            string msg = string.Join("\r", msh, qpd, rcp);

            return new Hl7Message
            {
                MessageType = "QBP^Q11",
                RawMessage = msg
            };
        }
    }
}
