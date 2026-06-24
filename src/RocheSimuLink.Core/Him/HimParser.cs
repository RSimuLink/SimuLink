using System.Text.RegularExpressions;
using RocheSimuLink.Models.Him;

namespace RocheSimuLink.Him
{
    /// <summary>
    /// Extracts a <see cref="HostInterfaceManual"/> from the text of an x800
    /// Data Manager Host Interface Manual.
    ///
    /// PdfPig collapses each page to a single string and reorders table columns
    /// (headers first, then cells), so this parser does not attempt full table
    /// reconstruction. Instead it anchors on stable landmarks — the
    /// "&lt;name&gt; - LIS mapping" assay headings, the column-tag terminators
    /// such as "(TCD-9-1)" / "(OBR-4 / TCD-1)" / "(OBX-3)", and the "y&lt;name&gt;"
    /// block-end markers — and pulls coded elements out by their HL7 shape.
    /// </summary>
    public static partial class HimParser
    {
        // id^text^system. Identifier and system are token-like; text allows
        // spaces and slashes (e.g. "CT/NG", "Real Time").
        [GeneratedRegex(@"([A-Za-z0-9\-]+)\^([A-Za-z0-9 /+.{}*]+?)\^(HL7\d+|99ROC|LN|LOINC|UCUM)")]
        private static partial Regex CodedElementRegex();

        // A sample-type row: <coded specimen> followed by its consumption
        // volume. The volume is the first integer after the code; trailing
        // alternates/footnotes (e.g. "850 / 150", a glued neighbour cell) are
        // ignored so each row yields one clean numeric volume.
        [GeneratedRegex(@"([A-Za-z0-9\-]+\^[A-Za-z0-9 /+.{}*]+?\^(?:HL7\d+|99ROC))\s*(\d+)")]
        private static partial Regex SampleTypeRowRegex();

        // A LOINC test code: ddddd-d^name^LN (name may contain hyphens, e.g. CHIKV-DENV).
        [GeneratedRegex(@"(\d{3,6}-\d)\^([A-Za-z0-9 /+.\-]+?)\^LN")]
        private static partial Regex LoincRegex();

        // A vendor target code: id^name^99ROC.
        [GeneratedRegex(@"([A-Za-z0-9\-]+)\^([A-Za-z0-9 /+.]+?)\^99ROC")]
        private static partial Regex VendorCodeRegex();

        // The MSH-9 message-type triple in an example message.
        [GeneratedRegex(@"MSH\|\^~\\&\|.*?\|\|([A-Z]{3})\^([A-Z0-9]{2,4})\^([A-Z0-9_]+)\|")]
        private static partial Regex MessageTypeRegex();

        // Manual version line: "Version 5.3".
        [GeneratedRegex(@"Host Interface Manual\s*·?\s*Version\s+([0-9.]+)")]
        private static partial Regex ManualVersionRegex();

        private static readonly string[] SegmentNames =
        {
            "MSH", "MSA", "QAK", "QPD", "RCP", "SPM", "SAC",
            "ORC", "OBR", "OBX", "TCD", "INV", "ERR", "NTE", "PID",
        };

        /// <summary>
        /// Parses the manual from its per-page text (as produced by
        /// <see cref="HimPdfReader.ReadPages(string)"/>).
        /// </summary>
        public static HostInterfaceManual Parse(IReadOnlyList<string> pages, string source = "")
        {
            ArgumentNullException.ThrowIfNull(pages);

            var manual = new HostInterfaceManual
            {
                Source = source,
                IngestedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
                ManualVersion = DetectVersion(pages),
                MessageTypes = ParseMessageTypes(pages),
                Assays = ParseAssays(pages),
            };

            return manual;
        }

        private static string DetectVersion(IReadOnlyList<string> pages)
        {
            foreach (var page in pages)
            {
                var match = ManualVersionRegex().Match(page);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return string.Empty;
        }

        // --- Message types --------------------------------------------------

        private static List<HimMessageType> ParseMessageTypes(IReadOnlyList<string> pages)
        {
            var byCode = new Dictionary<string, HimMessageType>(StringComparer.Ordinal);

            foreach (var page in pages)
            {
                foreach (Match m in MessageTypeRegex().Matches(page))
                {
                    var triple = $"{m.Groups[1].Value}^{m.Groups[2].Value}^{m.Groups[3].Value}";
                    var code = $"{m.Groups[1].Value}^{m.Groups[2].Value}";
                    if (byCode.ContainsKey(code))
                    {
                        continue;
                    }

                    byCode[code] = new HimMessageType
                    {
                        MessageType = triple,
                        Code = code,
                        ExampleMessage = ExtractExampleMessage(page, m.Index),
                    };
                }
            }

            return byCode.Values
                .OrderBy(t => t.Code, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Reconstructs the example message that starts at the MSH match by
        /// reading until a non-segment landmark and re-inserting CR boundaries
        /// before each known segment name.
        /// </summary>
        private static string ExtractExampleMessage(string page, int mshIndex)
        {
            // The example block runs from MSH up to a trailing marker. The
            // manual ends examples with a "k" glyph + "Example ... message"
            // (rendered either as "kExample" or "k Example").
            var tail = Regex.Match(page[mshIndex..], @"\bk\s*Example\b");
            var end = tail.Success ? mshIndex + tail.Index : page.Length;
            var block = page[mshIndex..end];

            return SplitIntoSegments(block);
        }

        private static string SplitIntoSegments(string block)
        {
            // Remove space artifacts introduced when PdfPig wraps a long line
            // (e.g. "P| 2.5.1" -> "P|2.5.1"). A field never legitimately starts
            // with a space, so dropping a space immediately after '|' is safe.
            var cleaned = Regex.Replace(block, @"\|\s+", "|");

            // Insert a separator before each segment name that begins a new
            // segment (a 3-letter code followed by '|'), except the leading MSH.
            var withBreaks = cleaned;
            foreach (var name in SegmentNames)
            {
                withBreaks = Regex.Replace(
                    withBreaks,
                    $@"(?<![A-Za-z0-9]){name}\|",
                    "\r" + name + "|");
            }

            var segments = new List<string>();
            foreach (var raw in withBreaks.Split('\r', StringSplitOptions.RemoveEmptyEntries))
            {
                var segment = raw.Trim();
                if (segment.Length <= 3)
                {
                    continue;
                }

                // The last segment carries trailing page-footer text after the
                // example ends; cut it at the first run of two+ spaces, which
                // never appears inside a real segment.
                var footer = Regex.Match(segment, @"\s{2,}");
                if (footer.Success)
                {
                    segment = segment[..footer.Index].TrimEnd();
                }

                segments.Add(segment);
            }

            return string.Join("\r", segments);
        }

        // --- Assays ---------------------------------------------------------

        private static List<AssayDefinition> ParseAssays(IReadOnlyList<string> pages)
        {
            var assays = new List<AssayDefinition>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var page in pages)
            {
                foreach (var assay in ParseAssaysOnPage(page))
                {
                    // The same assay heading can repeat (TOC, running headers);
                    // keep the first populated definition.
                    if (seen.Add(assay.Name))
                    {
                        assays.Add(assay);
                    }
                }
            }

            return assays
                .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static IEnumerable<AssayDefinition> ParseAssaysOnPage(string page)
        {
            // Assay blocks are introduced by "<name> - LIS mapping" and contain
            // the sample/test/target/result sections. Require the description
            // sentence ("is a ... assay") so we skip TOC and cross-references.
            var headingRegex = new Regex(@"([A-Za-z0-9/+\-]+) - LIS mapping");
            var matches = headingRegex.Matches(page);

            for (var i = 0; i < matches.Count; i++)
            {
                var name = matches[i].Value.Replace(" - LIS mapping", string.Empty).Trim();
                var start = matches[i].Index;
                var end = i + 1 < matches.Count ? matches[i + 1].Index : page.Length;
                var block = page[start..end];

                // Only treat as a real assay block when it carries the
                // descriptive sentence (filters out TOC / header noise) AND the
                // targets table tag (filters out changelog / cross-reference
                // blocks that mention an assay name but have no LIS mapping).
                var descMatch = Regex.Match(
                    block,
                    name + Regex.Escape(" - LIS mapping") + @"\s*(.*?\bassay\b[^.]*\.)",
                    RegexOptions.Singleline);
                if (!descMatch.Success || !block.Contains("(OBX-3)", StringComparison.Ordinal))
                {
                    continue;
                }

                var assay = new AssayDefinition
                {
                    Name = name,
                    Description = CollapseSpaces(descMatch.Groups[1].Value),
                };
                assay.IsQuantitative = assay.Description.Contains(
                    "quantitative", StringComparison.OrdinalIgnoreCase);

                assay.SampleTypes = ParseSampleTypes(block);
                assay.Tests = ParseTests(block);
                assay.Targets = ParseTargets(block);

                yield return assay;
            }
        }

        private static List<AssaySampleType> ParseSampleTypes(string block)
        {
            var result = new List<AssaySampleType>();
            var region = SliceBetween(block, "(TCD-9-1)", "sample types and input volume");
            if (region is null)
            {
                return result;
            }

            var lastEnd = 0;
            foreach (Match m in SampleTypeRowRegex().Matches(region))
            {
                var name = region[lastEnd..m.Index].Trim();
                result.Add(new AssaySampleType
                {
                    Name = CleanName(name),
                    SpecimenType = m.Groups[1].Value.Trim(),
                    VolumeMicroliters = m.Groups[2].Value.Trim(),
                });
                lastEnd = m.Index + m.Length;
            }

            return result;
        }

        private static List<AssayTest> ParseTests(string block)
        {
            var result = new List<AssayTest>();
            var region = SliceBetween(block, "(OBR-4 / TCD-1)", " tests");
            if (region is null)
            {
                return result;
            }

            var lastEnd = 0;
            foreach (Match m in LoincRegex().Matches(region))
            {
                var name = region[lastEnd..m.Index].Trim();
                result.Add(new AssayTest
                {
                    Name = CleanName(name),
                    UniversalServiceIdentifier =
                        $"{m.Groups[1].Value}^{m.Groups[2].Value.Trim()}^LN",
                });
                lastEnd = m.Index + m.Length;
            }

            return result;
        }

        private static List<AssayTarget> ParseTargets(string block)
        {
            var result = new List<AssayTarget>();
            var region = SliceBetween(block, "(OBX-3)", " targets");
            if (region is null)
            {
                return result;
            }

            var lastEnd = 0;
            foreach (Match m in VendorCodeRegex().Matches(region))
            {
                var name = region[lastEnd..m.Index].Trim();
                result.Add(new AssayTarget
                {
                    Name = CleanName(name),
                    ObservationIdentifier =
                        $"{m.Groups[1].Value}^{m.Groups[2].Value.Trim()}^99ROC",
                });
                lastEnd = m.Index + m.Length;
            }

            return result;
        }

        // --- Helpers --------------------------------------------------------

        /// <summary>
        /// Returns the substring between the first occurrence of
        /// <paramref name="afterTag"/> and the next occurrence of the block-end
        /// marker containing <paramref name="endMarker"/> (the "y&lt;name&gt; ..."
        /// caption). Returns null when the tag is absent.
        /// </summary>
        private static string? SliceBetween(string block, string afterTag, string endMarker)
        {
            var startTag = block.IndexOf(afterTag, StringComparison.Ordinal);
            if (startTag < 0)
            {
                return null;
            }

            var start = startTag + afterTag.Length;
            var end = block.IndexOf(endMarker, start, StringComparison.OrdinalIgnoreCase);
            if (end < 0)
            {
                end = block.Length;
            }

            // Back up over the "y<name>" caption lead-in if present.
            return block[start..end];
        }

        private static string CleanName(string raw)
        {
            // Names sometimes pick up trailing/leading section words or footnote
            // marks; keep the trailing token run that looks like a label.
            var cleaned = raw.Trim();
            // Drop a leading "y" block-marker glyph if it bled in.
            if (cleaned.StartsWith('y'))
            {
                cleaned = cleaned[1..].Trim();
            }

            return CollapseSpaces(cleaned);
        }

        private static string CollapseSpaces(string value) =>
            Regex.Replace(value, @"\s+", " ").Trim();
    }
}
