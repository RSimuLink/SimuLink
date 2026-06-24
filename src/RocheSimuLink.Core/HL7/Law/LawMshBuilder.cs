using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
{
    /// <summary>
    /// Builds the MSH segment shared by all x800DM IHE-LAW messages
    /// (QBP/RSP/OML/ORL/OUL/ACK). Field placement follows the Host Interface
    /// Manual: MSH-15/16 carry accept/application ack types on requests and are
    /// left empty on responses; MSH-21 carries the workflow profile identifier.
    /// </summary>
    public static class LawMshBuilder
    {
        public static string Build(LawMessageHeader header)
        {
            ArgumentNullException.ThrowIfNull(header);

            return new LawField("MSH")
                .Set(2, "^~\\&")
                .Set(3, header.SendingApplication)
                .Set(5, header.ReceivingApplication)
                .Set(7, header.MessageDateTime)
                .Set(9, header.MessageType)
                .Set(10, header.MessageControlId)
                .Set(11, header.ProcessingId)
                .Set(12, header.Version)
                .Set(15, header.AcceptAcknowledgment)
                .Set(16, header.ApplicationAcknowledgment)
                .Set(18, header.CharacterSet)
                .Set(21, header.ProfileIdentifier)
                .Render();
        }
    }
}
