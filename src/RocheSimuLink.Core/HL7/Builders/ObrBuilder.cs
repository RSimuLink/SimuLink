using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Builders
{
    public static class ObrBuilder
    {
        public static string Build(string orderId, string usi)
        {
            return $"OBR|1|{orderId}||{usi}";
        }
    }
}
