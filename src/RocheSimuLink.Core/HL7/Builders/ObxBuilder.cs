using RocheSimuLink.Models;

namespace RocheSimuLink.HL7.Builders
{
    public static class ObxBuilder
    {
        /// <summary>
        /// Builds an OBX with value (OBX-5) and abnormal flag (OBX-8).
        /// Result status (OBX-11) defaults to Final.
        /// </summary>
        public static string Build(Target target, string value, string interpretation)
        {
            return Build(target, value, interpretation, ResultStatus.Final, setId: 1);
        }

        /// <summary>
        /// Builds an OBX including result status (OBX-11).
        /// </summary>
        public static string Build(
            Target target,
            string value,
            string interpretation,
            ResultStatus status,
            int setId = 1,
            string valueType = "ST")
        {
            // Fields: 1 SetID | 2 ValueType | 3 ObsId | 4 SubId | 5 Value |
            //         6 Units | 7 RefRange | 8 AbnormalFlags | 9 | 10 | 11 Status
            return $"OBX|{setId}|{valueType}|{target.ObservationIdentifier}||{value}|||{interpretation}|||{status.ToHl7Code()}";
        }
    }
}
