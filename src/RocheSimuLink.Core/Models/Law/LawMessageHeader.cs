namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// The MSH context shared by all x800DM IHE-LAW messages. Per the Host
    /// Interface Manual, requests (QBP/OML/OUL) carry accept/application ack
    /// types (NE/AL) while responses (RSP/ORL/ACK) leave them empty, and the
    /// message-profile identifier (MSH-21) differs by workflow:
    /// LAB-27R^ROCHE (query), LAB-28^IHE (order), LAB-29^IHE (result/ack).
    /// </summary>
    public sealed class LawMessageHeader
    {
        /// <summary>MSH-3 sending application (e.g. "X800DM" or "Host").</summary>
        public string SendingApplication { get; set; } = string.Empty;

        /// <summary>MSH-5 receiving application (e.g. "Host" or "X800DM").</summary>
        public string ReceivingApplication { get; set; } = string.Empty;

        /// <summary>MSH-7 message date/time with offset (e.g. 20210915103321+0200).</summary>
        public string MessageDateTime { get; set; } = string.Empty;

        /// <summary>
        /// MSH-9 full message type triple (e.g. "QBP^Q11^QBP_Q11",
        /// "OML^O33^OML_O33", "ORL^O34^ORL_O42", "ACK^R22^ACK").
        /// </summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>MSH-10 message control id (a GUID in the traces).</summary>
        public string MessageControlId { get; set; } = string.Empty;

        /// <summary>MSH-11 processing id (fixed "P").</summary>
        public string ProcessingId { get; set; } = "P";

        /// <summary>MSH-12 HL7 version id (fixed "2.5.1").</summary>
        public string Version { get; set; } = "2.5.1";

        /// <summary>MSH-15 accept acknowledgment type ("NE" on requests, empty on responses).</summary>
        public string AcceptAcknowledgment { get; set; } = string.Empty;

        /// <summary>MSH-16 application acknowledgment type ("AL" on requests, empty on responses).</summary>
        public string ApplicationAcknowledgment { get; set; } = string.Empty;

        /// <summary>MSH-18 character set (fixed "UNICODE UTF-8").</summary>
        public string CharacterSet { get; set; } = "UNICODE UTF-8";

        /// <summary>MSH-21 message profile identifier (e.g. "LAB-27R^ROCHE").</summary>
        public string ProfileIdentifier { get; set; } = string.Empty;

        /// <summary>The bare message code^trigger (e.g. "QBP^Q11"), derived from MSH-9.</summary>
        public string MessageTypeCode
        {
            get
            {
                var parts = MessageType.Split('^');
                if (parts.Length >= 2)
                {
                    return $"{parts[0]}^{parts[1]}";
                }

                return MessageType;
            }
        }
    }
}
