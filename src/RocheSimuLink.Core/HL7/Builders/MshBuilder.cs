using RocheSimuLink.Models;

namespace RocheSimuLink.HL7.Builders
{
    public static class MshBuilder
    {
        /// <summary>
        /// Builds an MSH with the default SimuLink identities. Retained for the
        /// workflow builders and tests that don't carry connection settings.
        /// </summary>
        public static string Build(string messageType)
        {
            return Build(messageType, sendingApp: "SimuLink", sendingFacility: "Roche",
                receivingApp: "LIS", receivingFacility: "Hospital", version: "2.5.1");
        }

        /// <summary>
        /// Builds an MSH using identities from the Settings dialog.
        /// </summary>
        public static string Build(string messageType, ConnectionSettings settings)
        {
            return Build(messageType, settings.SendingApplication, settings.SendingFacility,
                settings.ReceivingApplication, settings.ReceivingFacility, settings.Hl7Version);
        }

        private static string Build(
            string messageType,
            string sendingApp,
            string sendingFacility,
            string receivingApp,
            string receivingFacility,
            string version)
        {
            return $"MSH|^~\\&|{sendingApp}|{sendingFacility}|{receivingApp}|{receivingFacility}|" +
                   $"{DateTime.Now:yyyyMMddHHmmss}||{messageType}|{Guid.NewGuid()}|P|{version}";
        }
    }
}
