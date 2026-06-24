using RocheSimuLink.Models;
using RocheSimuLink.Models.Him;

namespace RocheSimuLink.Him
{
    /// <summary>
    /// Loads a Host Interface Manual (from the PDF or a portable
    /// HIMdefinitions_00x.txt file) and applies its assay catalog onto the
    /// simulator's <see cref="SimuLinkSettings"/>, replacing the test, sample
    /// type, and volume lists. Connection/identity settings are left untouched.
    ///
    /// Kept in Core (no WinForms dependency) so it stays unit-testable on any
    /// platform; the UI only wires file pickers to these calls.
    /// </summary>
    public static class HimSettingsImporter
    {
        /// <summary>
        /// Reads a manual from either a ".pdf" (full ingestion via PdfPig) or a
        /// definitions ".txt" file (fast JSON load), chosen by file extension.
        /// </summary>
        public static HostInterfaceManual LoadManual(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var extension = Path.GetExtension(path);
            if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                var pages = HimPdfReader.ReadPages(path);
                return HimParser.Parse(pages, Path.GetFileName(path));
            }

            return HimDefinitionsStore.Load(path);
        }

        /// <summary>
        /// Replaces <paramref name="settings"/>'s catalog (TestTypes,
        /// SampleTypes, SampleVolumes) from the given manual. Returns a short
        /// summary describing what was imported.
        /// </summary>
        public static HimImportSummary Apply(SimuLinkSettings settings, HostInterfaceManual manual)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(manual);

            settings.TestTypes = HimCatalogMapper.ToTestTypes(manual);
            settings.SampleTypes = HimCatalogMapper.ToSampleTypes(manual);
            settings.SampleVolumes = HimCatalogMapper.ToSampleVolumes(manual);

            return new HimImportSummary
            {
                ManualVersion = manual.ManualVersion,
                AssayCount = manual.Assays.Count,
                TestTypeCount = settings.TestTypes.Count,
                SampleTypeCount = settings.SampleTypes.Count,
                SampleVolumeCount = settings.SampleVolumes.Count,
                MessageTypeCount = manual.MessageTypes.Count,
            };
        }

        /// <summary>Convenience: load from a path and apply in one call.</summary>
        public static HimImportSummary ImportFrom(SimuLinkSettings settings, string path)
        {
            var manual = LoadManual(path);
            return Apply(settings, manual);
        }
    }

    /// <summary>Counts describing the result of a HIM catalog import.</summary>
    public sealed class HimImportSummary
    {
        public string ManualVersion { get; init; } = string.Empty;
        public int AssayCount { get; init; }
        public int TestTypeCount { get; init; }
        public int SampleTypeCount { get; init; }
        public int SampleVolumeCount { get; init; }
        public int MessageTypeCount { get; init; }

        public override string ToString() =>
            $"HIM v{ManualVersion}: {AssayCount} assays \u2192 {TestTypeCount} tests, " +
            $"{SampleTypeCount} sample types, {SampleVolumeCount} volumes, " +
            $"{MessageTypeCount} message types.";
    }
}
