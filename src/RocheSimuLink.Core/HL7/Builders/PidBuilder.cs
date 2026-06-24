using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Builders
{
    public static class PidBuilder
    {
        public static string Build(string patientId)
        {
            return $"PID|1||{patientId}^^^SimuLink||Doe^John";
        }
    }
}
