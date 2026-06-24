using RocheSimuLink.Him;
using RocheSimuLink.Models.Him;
using Xunit;

namespace RocheSimuLink.Core.Tests.Him;

/// <summary>
/// Verifies the opt-in "remember catalog" persistence: save, load, clear, and
/// resilience to a missing/corrupt file. Uses a temp base directory so the
/// real per-user app-data folder is never touched.
/// </summary>
public class HimCatalogPersistenceTests : IDisposable
{
    private readonly string _baseDir =
        Path.Combine(Path.GetTempPath(), $"himcat_{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_baseDir))
        {
            Directory.Delete(_baseDir, recursive: true);
        }
    }

    private static HostInterfaceManual SampleManual() => new()
    {
        ManualVersion = "5.3",
        Assays =
        {
            new AssayDefinition { Name = "BKV", IsQuantitative = true },
        },
    };

    [Fact]
    public void SaveThenLoad_RoundTrips()
    {
        Assert.Null(HimCatalogPersistence.Load(_baseDir));
        Assert.False(HimCatalogPersistence.Exists(_baseDir));

        var path = HimCatalogPersistence.Save(SampleManual(), _baseDir);

        Assert.True(File.Exists(path));
        Assert.True(HimCatalogPersistence.Exists(_baseDir));

        var loaded = HimCatalogPersistence.Load(_baseDir);
        Assert.NotNull(loaded);
        Assert.Equal("5.3", loaded!.ManualVersion);
        Assert.Single(loaded.Assays);
    }

    [Fact]
    public void Clear_RemovesFile()
    {
        HimCatalogPersistence.Save(SampleManual(), _baseDir);

        Assert.True(HimCatalogPersistence.Clear(_baseDir));
        Assert.False(HimCatalogPersistence.Exists(_baseDir));
        Assert.Null(HimCatalogPersistence.Load(_baseDir));

        // Clearing again is a no-op.
        Assert.False(HimCatalogPersistence.Clear(_baseDir));
    }

    [Fact]
    public void Load_ReturnsNull_ForCorruptFile()
    {
        var path = HimCatalogPersistence.GetPath(_baseDir);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ this is not valid json");

        // A corrupt file must not throw on startup; it is treated as "none".
        Assert.Null(HimCatalogPersistence.Load(_baseDir));
    }
}
