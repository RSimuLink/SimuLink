using RocheSimuLink.Models;
using RocheSimuLink.Models.Him;

namespace RocheSimuLink.Him
{
    /// <summary>
    /// Projects the ingested HIM assay catalog onto the UI's existing domain
    /// models (<see cref="TestType"/>, <see cref="Target"/>,
    /// <see cref="SampleType"/>, <see cref="SampleVolume"/>) so the simulator
    /// can populate its dropdowns and build messages from the manual rather than
    /// from hand-maintained configuration.
    /// </summary>
    public static class HimCatalogMapper
    {
        /// <summary>
        /// Maps one assay to UI test types. Each <see cref="AssayTest"/> becomes
        /// a <see cref="TestType"/> carrying the assay's targets and the volumes
        /// derived from its sample types.
        /// </summary>
        public static List<TestType> ToTestTypes(AssayDefinition assay)
        {
            ArgumentNullException.ThrowIfNull(assay);

            var targets = assay.Targets.Select(ToTarget).ToList();
            var volumes = assay.SampleTypes
                .Select(s => s.VolumeMicroliters)
                .Where(v => v.Length > 0)
                .Distinct()
                .Select(v => new SampleVolume { Volume = $"{v} uL" })
                .ToList();

            return assay.Tests.Select(test => new TestType
            {
                Name = test.Name,
                UniversalServiceIdentifier = test.UniversalServiceIdentifier,
                // Clone targets per test so edits in the UI don't alias.
                Targets = targets.Select(CloneTarget).ToList(),
                AllowedVolumes = volumes.Select(v => new SampleVolume { Volume = v.Volume }).ToList(),
            }).ToList();
        }

        /// <summary>Maps an assay's sample types to distinct UI sample types.</summary>
        public static List<SampleType> ToSampleTypes(AssayDefinition assay)
        {
            ArgumentNullException.ThrowIfNull(assay);

            return assay.SampleTypes
                .Select(s => new SampleType
                {
                    DisplayName = s.Name,
                    // SPM-4 identifier component (e.g. "PLAS" from "PLAS^plasma^HL70487").
                    Hl7Code = s.SpecimenType.Split('^')[0],
                    // Full SPM-4 coded element (e.g. "PLAS^plasma^HL70487").
                    SpecimenCode = s.SpecimenType,
                })
                .ToList();
        }

        /// <summary>Maps every assay in the manual to UI test types.</summary>
        public static List<TestType> ToTestTypes(HostInterfaceManual manual)
        {
            ArgumentNullException.ThrowIfNull(manual);
            return manual.Assays.SelectMany(ToTestTypes).ToList();
        }

        /// <summary>Maps the manual's assays to a distinct, de-duplicated sample-type list.</summary>
        public static List<SampleType> ToSampleTypes(HostInterfaceManual manual)
        {
            ArgumentNullException.ThrowIfNull(manual);

            var byCode = new Dictionary<string, SampleType>(StringComparer.OrdinalIgnoreCase);
            foreach (var assay in manual.Assays)
            {
                foreach (var sample in ToSampleTypes(assay))
                {
                    if (sample.Hl7Code.Length > 0 && !byCode.ContainsKey(sample.Hl7Code))
                    {
                        byCode[sample.Hl7Code] = sample;
                    }
                }
            }

            return byCode.Values
                .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Maps the manual's assays to a distinct, sorted list of sample volume
        /// options (e.g. "850 uL"), as used by the volume dropdown.
        /// </summary>
        public static List<SampleVolume> ToSampleVolumes(HostInterfaceManual manual)
        {
            ArgumentNullException.ThrowIfNull(manual);

            return manual.Assays
                .SelectMany(a => a.SampleTypes)
                .Select(s => s.VolumeMicroliters)
                .Where(v => v.Length > 0)
                .Distinct()
                .OrderBy(v => int.TryParse(v, out var n) ? n : int.MaxValue)
                .ThenBy(v => v, StringComparer.Ordinal)
                .Select(v => new SampleVolume { Volume = $"{v} uL" })
                .ToList();
        }

        private static Target ToTarget(AssayTarget target) => new()
        {
            Name = target.Name,
            ObservationIdentifier = target.ObservationIdentifier,
            ObservationValues = new List<string>(target.ObservationValues),
            InterpretationCodes = new List<string>(target.InterpretationCodes),
        };

        private static Target CloneTarget(Target target) => new()
        {
            Name = target.Name,
            ObservationIdentifier = target.ObservationIdentifier,
            ObservationValues = new List<string>(target.ObservationValues),
            InterpretationCodes = new List<string>(target.InterpretationCodes),
        };
    }
}
