using RocheLIT.Models;

namespace RocheLIT.Services
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
            return VolumesFor(test, sampleType: null, catalogVolumes);
        }

        /// <summary>
        /// The sample volumes to offer after a sample type is selected. A
        /// sample type from a HIM assay row can constrain the volume to the
        /// value in "Sample types and input volume"; otherwise the test-level
        /// volume list or global catalog applies.
        /// </summary>
        public static IReadOnlyList<SampleVolume> VolumesFor(
            TestType? test,
            SampleType? sampleType,
            IReadOnlyList<SampleVolume> catalogVolumes)
        {
            ArgumentNullException.ThrowIfNull(catalogVolumes);

            if (sampleType is not null && sampleType.AllowedVolumes.Count > 0)
            {
                return sampleType.AllowedVolumes;
            }

            if (test is not null && test.AllowedVolumes.Count > 0)
            {
                return test.AllowedVolumes;
            }

            return catalogVolumes;
        }

        /// <summary>
        /// The sample types to offer for the selected test. Manual-derived
        /// tests carry the assay-specific sample type list; older/custom
        /// settings fall back to the global catalog.
        /// </summary>
        public static IReadOnlyList<SampleType> SampleTypesFor(
            TestType? test, IReadOnlyList<SampleType> catalogSampleTypes)
        {
            ArgumentNullException.ThrowIfNull(catalogSampleTypes);

            if (test is not null && test.AllowedSampleTypes.Count > 0)
            {
                return test.AllowedSampleTypes;
            }

            return catalogSampleTypes;
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
