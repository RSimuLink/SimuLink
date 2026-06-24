namespace RocheSimuLink.Models
{
    /// <summary>
    /// A configurable sample volume option (e.g. "200 uL").
    /// </summary>
    public class SampleVolume
    {
        /// <summary>Volume label shown in the UI (e.g. "200 uL").</summary>
        public string Volume { get; set; } = string.Empty;

        public override string ToString() => Volume;
    }
}
