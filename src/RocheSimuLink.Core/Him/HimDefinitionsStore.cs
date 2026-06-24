using System.Text.Json;
using System.Text.RegularExpressions;
using RocheSimuLink.Models.Him;

namespace RocheSimuLink.Him
{
    /// <summary>
    /// Persists and loads the parsed Host Interface Manual as a portable
    /// definitions file (JSON content with a ".txt" extension by request).
    ///
    /// The convention is "HIMdefinitions_00x.txt", where the user picks the
    /// number (e.g. HIMdefinitions_001.txt). This lets one machine ingest the
    /// PDF once and other machines load the much smaller definitions file
    /// without needing PdfPig or the original manual.
    /// </summary>
    public static partial class HimDefinitionsStore
    {
        public const string FilePrefix = "HIMdefinitions_";
        public const string FileExtension = ".txt";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        [GeneratedRegex(@"^HIMdefinitions_(\d{3,})\.txt$", RegexOptions.IgnoreCase)]
        private static partial Regex DefinitionsFileRegex();

        /// <summary>Builds a definitions file name for a given number (e.g. 1 -> "HIMdefinitions_001.txt").</summary>
        public static string FileNameFor(int number) =>
            $"{FilePrefix}{number:000}{FileExtension}";

        /// <summary>True when the file name matches the HIMdefinitions_00x.txt convention.</summary>
        public static bool IsDefinitionsFileName(string fileName) =>
            DefinitionsFileRegex().IsMatch(Path.GetFileName(fileName));

        /// <summary>Serializes the manual to the given path.</summary>
        public static void Save(HostInterfaceManual manual, string path)
        {
            ArgumentNullException.ThrowIfNull(manual);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var json = Serialize(manual);
            File.WriteAllText(path, json);
        }

        /// <summary>Serializes the manual to a JSON string.</summary>
        public static string Serialize(HostInterfaceManual manual)
        {
            ArgumentNullException.ThrowIfNull(manual);
            return JsonSerializer.Serialize(manual, JsonOptions);
        }

        /// <summary>Loads a manual from a definitions file path.</summary>
        public static HostInterfaceManual Load(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            return Deserialize(File.ReadAllText(path));
        }

        /// <summary>Deserializes a manual from a JSON string.</summary>
        public static HostInterfaceManual Deserialize(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);
            return JsonSerializer.Deserialize<HostInterfaceManual>(json, JsonOptions)
                ?? throw new InvalidDataException("Definitions file did not contain a valid manual.");
        }

        /// <summary>
        /// Ingests a HIM PDF and writes the definitions file beside it (or in
        /// <paramref name="outputDirectory"/> when provided), returning the
        /// parsed manual and the file path written.
        /// </summary>
        public static (HostInterfaceManual Manual, string Path) IngestPdf(
            string pdfPath, int number = 1, string? outputDirectory = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);

            var pages = HimPdfReader.ReadPages(pdfPath);
            var manual = HimParser.Parse(pages, Path.GetFileName(pdfPath));

            var dir = outputDirectory ?? Path.GetDirectoryName(Path.GetFullPath(pdfPath)) ?? ".";
            var outPath = Path.Combine(dir, FileNameFor(number));
            Save(manual, outPath);

            return (manual, outPath);
        }
    }
}
