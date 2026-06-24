using RocheSimuLink.Models.Him;

namespace RocheSimuLink.Him
{
    /// <summary>
    /// Persists a "remembered" assay catalog so it survives application
    /// restarts. The catalog is stored as a definitions file in a per-user
    /// application-data folder; on startup the app loads it (when present) to
    /// pre-populate the catalog without re-ingesting a HIM PDF.
    ///
    /// This is opt-in: importing a HIM only writes here when the user ticks the
    /// "remember" checkbox. Clearing the option deletes the stored file so the
    /// next start falls back to the built-in seed catalog.
    /// </summary>
    public static class HimCatalogPersistence
    {
        private const string FolderName = "RocheSimuLink";
        private const string FileName = "HIMdefinitions_active.txt";

        /// <summary>
        /// The path of the remembered-catalog file. Override the base folder via
        /// <paramref name="baseDirectory"/> (used by tests); defaults to the
        /// per-user ApplicationData location.
        /// </summary>
        public static string GetPath(string? baseDirectory = null)
        {
            var root = baseDirectory ?? Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);
            return Path.Combine(root, FolderName, FileName);
        }

        /// <summary>True when a remembered catalog file exists.</summary>
        public static bool Exists(string? baseDirectory = null) =>
            File.Exists(GetPath(baseDirectory));

        /// <summary>
        /// Saves the manual as the remembered catalog, creating the folder if
        /// needed. Returns the path written.
        /// </summary>
        public static string Save(HostInterfaceManual manual, string? baseDirectory = null)
        {
            ArgumentNullException.ThrowIfNull(manual);

            var path = GetPath(baseDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            HimDefinitionsStore.Save(manual, path);
            return path;
        }

        /// <summary>
        /// Loads the remembered catalog, or null when none has been saved or the
        /// stored file is unreadable/corrupt (so a bad file never blocks startup).
        /// </summary>
        public static HostInterfaceManual? Load(string? baseDirectory = null)
        {
            var path = GetPath(baseDirectory);
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return HimDefinitionsStore.Load(path);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException
                or System.Text.Json.JsonException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the remembered catalog if present. Returns true when a file
        /// was removed.
        /// </summary>
        public static bool Clear(string? baseDirectory = null)
        {
            var path = GetPath(baseDirectory);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }
    }
}
