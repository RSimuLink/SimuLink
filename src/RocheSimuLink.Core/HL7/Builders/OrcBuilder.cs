using System;
using System.Collections.Generic;
using System.Text;

namespace RocheSimuLink.HL7.Builders
{
    public static class OrcBuilder
    {
        public static string Build(string orderId)
        {
            // ORC-1 = OK (Order acknowledged)
            // ORC-2 = Placer Order Number
            return $"ORC|OK|{orderId}";
        }
    }
}
