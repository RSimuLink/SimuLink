namespace RocheLIT.Models.Ucap
{
    /// <summary>One user-configured UCAP target and its qualitative result.</summary>
    public sealed class UcapTargetResult
    {
        /// <summary>Target name, rendered as OBX-3 id and text.</summary>
        public string TargetName { get; set; } = string.Empty;

        /// <summary>Displayed OBX-5 value: Reactive or Non-Reactive.</summary>
        public string ResultValue { get; set; } = string.Empty;
    }
}
