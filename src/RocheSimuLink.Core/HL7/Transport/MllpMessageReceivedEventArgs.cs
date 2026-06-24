using RocheSimuLink.HL7.Parsers;

namespace RocheSimuLink.HL7.Transport
{
    /// <summary>
    /// Carries an inbound HL7 message to subscribers and collects the
    /// acknowledgement they want returned to the sender.
    /// </summary>
    public sealed class MllpMessageReceivedEventArgs : EventArgs
    {
        public MllpMessageReceivedEventArgs(string rawMessage, ParsedHl7Message parsed)
        {
            RawMessage = rawMessage;
            Parsed = parsed;
        }

        /// <summary>The raw HL7 text received (MLLP framing removed).</summary>
        public string RawMessage { get; }

        /// <summary>The parsed representation of <see cref="RawMessage"/>.</summary>
        public ParsedHl7Message Parsed { get; }

        /// <summary>
        /// HL7 acknowledgement to send back. When left null, the listener sends
        /// a default ACK.
        /// </summary>
        public string? Acknowledgement { get; set; }
    }
}
