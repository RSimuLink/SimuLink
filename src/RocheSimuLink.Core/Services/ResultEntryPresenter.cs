using RocheSimuLink.Models;

namespace RocheSimuLink.Services
{
    /// <summary>
    /// Pure presentation logic for the result-entry panel, factored out of the
    /// WinForms <c>MainForm</c> so it can be unit-tested without a UI. Each
    /// method maps the current selection to what the dropdowns/fields should
    /// show, with no dependency on any control.
    /// </summary>
    public static class ResultEntryPresenter
    {
        /// <summary>
        /// The result values to offer for a test: the first target's OBX-5
        /// values (the panel result the user edits). Empty when the test has no
        /// targets or the lead target has no configured values.
        /// </summary>
        public static IReadOnlyList<string> ResultValuesFor(TestType? test)
        {
            if (test is null || test.Targets.Count == 0)
            {
                return Array.Empty<string>();
            }

            return test.Targets[0].ObservationValues;
        }

        /// <summary>
        /// The sample volumes to offer for a test. A test that declares its own
        /// <see cref="TestType.AllowedVolumes"/> constrains the choice; otherwise
        /// the catalog-wide <paramref name="catalogVolumes"/> apply.
        /// </summary>
        public static IReadOnlyList<SampleVolume> VolumesFor(
            TestType? test, IReadOnlyList<SampleVolume> catalogVolumes)
        {
            ArgumentNullException.ThrowIfNull(catalogVolumes);

            if (test is not null && test.AllowedVolumes.Count > 0)
            {
                return test.AllowedVolumes;
            }

            return catalogVolumes;
        }

        /// <summary>
        /// Resolves the abnormal flag from the panel's checkbox states. Critical
        /// takes precedence, then High, then Low; otherwise Normal. This mirrors
        /// the mutually-exclusive intent of the checkboxes when more than one is
        /// somehow set.
        /// </summary>
        public static ResultFlag ResolveFlag(bool critical, bool high, bool low)
        {
            if (critical) return ResultFlag.Critical;
            if (high) return ResultFlag.High;
            if (low) return ResultFlag.Low;
            return ResultFlag.Normal;
        }

        /// <summary>
        /// The OBX-5 value actually sent: the user's selection when present,
        /// else the lead target's first configured value, else "N/A" so a
        /// message is never built with an empty result.
        /// </summary>
        public static string EffectiveResultValue(TestType? test, string? selectedValue)
        {
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                return selectedValue;
            }

            var fallback = test is { Targets.Count: > 0 }
                ? test.Targets[0].ObservationValues.FirstOrDefault()
                : null;

            return fallback ?? "N/A";
        }

        /// <summary>
        /// Whether the current selection is complete enough to build and send a
        /// result: a test with at least one target, and a sample type.
        /// </summary>
        public static bool CanSend(TestType? test, SampleType? sampleType) =>
            test is { Targets.Count: > 0 } && sampleType is not null;
    }
}
