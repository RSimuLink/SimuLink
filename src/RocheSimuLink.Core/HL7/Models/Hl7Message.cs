using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Models
{
    public class Hl7Message
    {
        public string MessageType { get; set; } = string.Empty;   // e.g. "OUL^R22"
        public string RawMessage { get; set; } = string.Empty;    // full HL7 text
    }
}
