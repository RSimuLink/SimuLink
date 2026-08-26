using System.Text.Json;
using RocheLIT.Models;

namespace RocheLIT.Services
{
    /// <summary>
    /// Persists LIS connection settings outside the installer directory so test
    /// network settings survive application restarts without being hardcoded.
    /// </summary>
    public static class ConnectionSettingsPersistence
    {
        public const string SettingsDirectoryEnvironmentVariable = "ROCHE_LIT_SETTINGS_DIR";

        private const string SettingsDirectoryName = "RocheLIT";
        private const string SettingsFileName = "connection-settings.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
        };

        public static ConnectionSettings? Load()
        {
            var path = SettingsFilePath();
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<ConnectionSettings>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public static void Save(ConnectionSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var directory = SettingsDirectory();
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(Path.Combine(directory, SettingsFileName), json);
        }

        private static string SettingsFilePath() =>
            Path.Combine(SettingsDirectory(), SettingsFileName);

        private static string SettingsDirectory()
        {
            var overrideDirectory = Environment.GetEnvironmentVariable(SettingsDirectoryEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(overrideDirectory))
            {
                return overrideDirectory;
            }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData.Length > 0 ? appData : AppContext.BaseDirectory, SettingsDirectoryName);
        }
    }
}
