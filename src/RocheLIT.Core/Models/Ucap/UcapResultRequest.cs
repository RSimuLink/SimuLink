using RocheLIT.Models;

namespace RocheLIT.Models.Ucap
{
    /// <summary>Input required to build a UCAP OUL^R22 result message.</summary>
    public sealed class UcapResultRequest
    {
        public string SampleId { get; set; } = string.Empty;

        /// <summary>The user-entered part after the fixed U_ prefix.</summary>
        public string TestNameSuffix { get; set; } = string.Empty;

        /// <summary>The 7-digit UCAP Universal Service Identifier.</summary>
        public string UniversalServiceId { get; set; } = string.Empty;

        public SampleType SampleType { get; set; } = new();
        public string SampleVolume { get; set; } = string.Empty;
        public string RackId { get; set; } = string.Empty;
        public string CarrierPosition { get; set; } = string.Empty;
        public List<UcapTargetResult> Targets { get; set; } = new();
    }
}
