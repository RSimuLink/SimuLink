using RocheSimuLink.Logging;
using Xunit;

namespace RocheSimuLink.Core.Tests.Logging;

public class ActivityLogTests
{
    [Fact]
    public void Add_StoresEntryWithSeverity()
    {
        var log = new ActivityLog();

        log.Success("connected");
        log.Error("boom");

        Assert.Equal(2, log.Entries.Count);
        Assert.Equal(LogSeverity.Success, log.Entries[0].Severity);
        Assert.Equal(LogSeverity.Error, log.Entries[1].Severity);
    }

    [Fact]
    public void EntryAdded_FiresForEachEntry()
    {
        var log = new ActivityLog();
        var received = new List<string>();
        log.EntryAdded += (_, e) => received.Add(e.Message);

        log.Info("one");
        log.Warning("two");

        Assert.Equal(new[] { "one", "two" }, received);
    }

    [Fact]
    public void ToString_UsesInjectedClock()
    {
        var fixedTime = new DateTimeOffset(2026, 6, 24, 13, 5, 9, TimeSpan.Zero);
        var log = new ActivityLog { Clock = () => fixedTime };

        var entry = log.Success("ready");

        Assert.Equal("13:05:09 - ready", entry.ToString());
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var log = new ActivityLog();
        log.Info("x");

        log.Clear();

        Assert.Empty(log.Entries);
    }
}
