using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Builders
{
    public static class SpmBuilder
    {
        public static string Build(string sampleId, string sampleTypeCode)
        {
            return $"SPM|1|{sampleId}||{sampleTypeCode}";
        }
    }
}
