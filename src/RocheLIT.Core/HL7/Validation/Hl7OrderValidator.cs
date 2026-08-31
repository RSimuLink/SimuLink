using RocheLIT.HL7.Parsers;

namespace RocheLIT.HL7.Validation
{
    /// <summary>
    /// Validates inbound order messages against the LAW field positions used by
    /// the simulator. The current HIM ingestion provides assay/catalog data but
    /// not a full machine-readable segment table, so fixed LAW segment rules are
    /// used for the structural fields the app consumes.
    /// </summary>
    public static class Hl7OrderValidator
    {
        private static readonly IReadOnlyDictionary<string, SegmentRule> Rules =
            new Dictionary<string, SegmentRule>(StringComparer.Ordinal)
            {
                ["MSH"] = new(21),
                ["SPM"] = new(11),
                ["SAC"] = new(11, new Dictionary<int, int>
                {
                    [10] = 80,
                    [11] = 16,
                }),
                ["ORC"] = new(9),
                ["OBR"] = new(4),
                ["TCD"] = new(9),
            };

        private static readonly string[] RequiredSegments =
        {
            "MSH", "SPM", "SAC", "ORC", "OBR", "TCD",
        };

        public static IReadOnlyList<Hl7ValidationIssue> Validate(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var issues = new List<Hl7ValidationIssue>();
            var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var segment in message.Segments)
            {
                occurrences.TryGetValue(segment.Name, out var occurrence);
                occurrence++;
                occurrences[segment.Name] = occurrence;

                if (!Rules.TryGetValue(segment.Name, out var rule))
                {
                    continue;
                }

                if (segment.Name == "MSH")
                {
                    ValidateMshBoundary(segment, occurrence, issues);
                }

                ValidateFieldCount(segment, occurrence, rule, issues);
                ValidateFieldLengths(segment, occurrence, rule, issues);
            }

            ValidateRequiredSegments(occurrences, issues);

            return issues;
        }

        private static void ValidateMshBoundary(
            Hl7Segment segment,
            int occurrence,
            List<Hl7ValidationIssue> issues)
        {
            var raw = segment.RawText;
            var encodingChars = raw.Length >= 8 ? raw.Substring(4, 4) : segment.Field(2);
            if (encodingChars != "^~\\&")
            {
                issues.Add(new Hl7ValidationIssue(
                    "MSH",
                    occurrence,
                    2,
                    "MSH-2 must be ^~\\& (ASCII 094, 126, 092, 038)."));
            }

            var fieldSeparator = segment.Field(1);
            var expectedSeparator = fieldSeparator.Length == 1 ? fieldSeparator[0] : '|';
            if (raw.Length <= 8 || raw[8] != expectedSeparator)
            {
                issues.Add(new Hl7ValidationIssue(
                    "MSH",
                    occurrence,
                    3,
                    "missing field separator after MSH-2; MSH-3 must start after ^~\\&|."));
            }
        }

        private static void ValidateRequiredSegments(
            IReadOnlyDictionary<string, int> occurrences,
            List<Hl7ValidationIssue> issues)
        {
            foreach (var segmentName in RequiredSegments)
            {
                if (occurrences.ContainsKey(segmentName))
                {
                    continue;
                }

                issues.Add(new Hl7ValidationIssue(
                    segmentName,
                    1,
                    0,
                    "required segment is missing from the inbound LIS order."));
            }
        }

        private static void ValidateFieldCount(
            Hl7Segment segment,
            int occurrence,
            SegmentRule rule,
            List<Hl7ValidationIssue> issues)
        {
            if (segment.FieldCount >= rule.ExpectedFieldCount)
            {
                return;
            }

            var firstMissing = segment.FieldCount + 1;
            issues.Add(new Hl7ValidationIssue(
                segment.Name,
                occurrence,
                firstMissing,
                $"segment has {segment.FieldCount} field(s), but expected fields through " +
                $"{segment.Name}-{rule.ExpectedFieldCount}; possible missing '|' before " +
                $"{segment.Name}-{firstMissing}."));
        }

        private static void ValidateFieldLengths(
            Hl7Segment segment,
            int occurrence,
            SegmentRule rule,
            List<Hl7ValidationIssue> issues)
        {
            foreach (var (fieldPosition, maxLength) in rule.MaxLengths)
            {
                var value = segment.Field(fieldPosition);
                if (value.Length <= maxLength)
                {
                    continue;
                }

                issues.Add(new Hl7ValidationIssue(
                    segment.Name,
                    occurrence,
                    fieldPosition,
                    $"field length is {value.Length}, maximum allowed is {maxLength}."));
            }
        }

        private sealed class SegmentRule
        {
            public SegmentRule(
                int expectedFieldCount,
                IReadOnlyDictionary<int, int>? maxLengths = null)
            {
                ExpectedFieldCount = expectedFieldCount;
                MaxLengths = maxLengths ?? new Dictionary<int, int>();
            }

            public int ExpectedFieldCount { get; }
            public IReadOnlyDictionary<int, int> MaxLengths { get; }
        }
    }
}
