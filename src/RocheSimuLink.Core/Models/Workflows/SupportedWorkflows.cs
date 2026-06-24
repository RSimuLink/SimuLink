namespace RocheSimuLink.Models.Workflows
{
    /// <summary>
    /// Known HL7 workflows the simulator can build. Values map to message type codes.
    /// </summary>
    public enum SupportedWorkflows
    {
        /// <summary>Unsolicited result (laboratory) — OUL^R22.</summary>
        OulR22,

        /// <summary>Laboratory order — OML^O33.</summary>
        OmlO33,

        /// <summary>Laboratory order response — ORL^O34.</summary>
        OrlO34,

        /// <summary>Query response — RSP^K11.</summary>
        RspK11,

        /// <summary>Query by parameter — QBP^Q11.</summary>
        QbpQ11,

        /// <summary>Acknowledgement — ACK^R22.</summary>
        AckR22
    }
}
