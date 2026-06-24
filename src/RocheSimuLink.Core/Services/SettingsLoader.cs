using RocheSimuLink.Him;
using RocheSimuLink.Models;
using RocheSimuLink.Models.Workflows;

namespace RocheSimuLink.Services
{
    /// <summary>
    /// Provides the simulator configuration. The assay catalog (tests, sample
    /// types, volumes) is sourced from the bundled Host Interface Manual PDF so
    /// the app ships with a real, manual-derived default rather than hand-kept
    /// sample data. A previously "remembered" catalog (see
    /// <see cref="HimCatalogPersistence"/>) overlays the default when the user
    /// opted to persist an imported manual, so it survives restarts.
    ///
    /// If the bundled PDF is missing or fails to parse, the catalog is left
    /// empty rather than blocking startup; the user can still import a manual
    /// from the Settings dialog. Connection/identity settings and the supported
    /// workflow list come from the built-in seed.
    /// </summary>
    public static class SettingsLoader
    {
        /// <summary>File name of the Host Interface Manual bundled with the app.</summary>
        public const string BundledManualFileName = "HIMv2_1.pdf";

        public static SimuLinkSettings Load()
        {
            var settings = BuildSeed();

            // Default catalog: parse the bundled HIM. A missing/corrupt PDF must
            // not block startup, so failures fall back to an empty catalog.
            var bundled = LoadBundledManual();
            if (bundled is not null)
            {
                HimSettingsImporter.Apply(settings, bundled);
            }

            // Overlay a remembered catalog so a user-imported HIM survives
            // restart, replacing the bundled default.
            var remembered = HimCatalogPersistence.Load();
            if (remembered is not null)
            {
                HimSettingsImporter.Apply(settings, remembered);
            }

            return settings;
        }

        /// <summary>
        /// Parses the bundled manual, or returns null when it cannot be located
        /// or read (so the app still starts with an empty catalog).
        /// </summary>
        private static Models.Him.HostInterfaceManual? LoadBundledManual()
        {
            var path = LocateBundledManual();
            if (path is null)
            {
                return null;
            }

            try
            {
                return HimSettingsImporter.LoadManual(path);
            }
            catch (Exception)
            {
                // Any parse/IO failure degrades to an empty catalog rather than
                // crashing startup; the user can import a manual manually.
                return null;
            }
        }

        /// <summary>
        /// Finds the bundled manual: first next to the running binary (the
        /// shipped layout), then by walking up to the repository root (dev/test
        /// runs from bin/Debug/...).
        /// </summary>
        private static string? LocateBundledManual()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, BundledManualFileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                dir = dir.Parent;
            }

            return null;
        }

        private static SimuLinkSettings BuildSeed()
        {
            var workflows = new List<Workflow>
            {
                new() { Kind = SupportedWorkflows.OulR22, MessageType = "OUL^R22", Description = "Unsolicited result" },
                new() { Kind = SupportedWorkflows.OmlO33, MessageType = "OML^O33", Description = "Laboratory order" },
                new() { Kind = SupportedWorkflows.OrlO34, MessageType = "ORL^O34", Description = "Laboratory order response" },
                new() { Kind = SupportedWorkflows.RspK11, MessageType = "RSP^K11", Description = "Query response" },
                new() { Kind = SupportedWorkflows.QbpQ11, MessageType = "QBP^Q11", Description = "Query by parameter" },
                new() { Kind = SupportedWorkflows.AckR22, MessageType = "ACK^R22", Description = "Acknowledgement" },
            };

            return new SimuLinkSettings
            {
                Workflows = workflows,
            };
        }
    }
}
