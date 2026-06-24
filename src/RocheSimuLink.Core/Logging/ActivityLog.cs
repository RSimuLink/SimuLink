namespace RocheSimuLink.Logging
{
    /// <summary>Severity of an activity-log entry, used to pick the UI icon.</summary>
    public enum LogSeverity
    {
        /// <summary>Success / completed action (green check in the mockup).</summary>
        Success,

        /// <summary>Neutral informational entry.</summary>
        Info,

        /// <summary>Recoverable problem worth highlighting.</summary>
        Warning,

        /// <summary>Failure.</summary>
        Error,
    }

    /// <summary>A single timestamped activity-log entry.</summary>
    public sealed class ActivityLogEntry
    {
        public ActivityLogEntry(LogSeverity severity, string message, DateTimeOffset timestamp)
        {
            Severity = severity;
            Message = message;
            Timestamp = timestamp;
        }

        public LogSeverity Severity { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }

        /// <summary>Formats as "HH:mm:ss - message" for plain-text display.</summary>
        public override string ToString() => $"{Timestamp:HH:mm:ss} - {Message}";
    }

    /// <summary>
    /// In-memory, thread-safe activity log. The UI subscribes to
    /// <see cref="EntryAdded"/> to render entries with severity icons.
    /// </summary>
    public sealed class ActivityLog
    {
        private readonly object _gate = new();
        private readonly List<ActivityLogEntry> _entries = new();

        /// <summary>Provides the current time; overridable for tests.</summary>
        public Func<DateTimeOffset> Clock { get; init; } = () => DateTimeOffset.Now;

        /// <summary>Raised whenever a new entry is appended.</summary>
        public event EventHandler<ActivityLogEntry>? EntryAdded;

        /// <summary>Snapshot of all entries in order.</summary>
        public IReadOnlyList<ActivityLogEntry> Entries
        {
            get
            {
                lock (_gate)
                {
                    return _entries.ToArray();
                }
            }
        }

        public ActivityLogEntry Add(LogSeverity severity, string message)
        {
            var entry = new ActivityLogEntry(severity, message, Clock());
            lock (_gate)
            {
                _entries.Add(entry);
            }

            EntryAdded?.Invoke(this, entry);
            return entry;
        }

        public ActivityLogEntry Success(string message) => Add(LogSeverity.Success, message);
        public ActivityLogEntry Info(string message) => Add(LogSeverity.Info, message);
        public ActivityLogEntry Warning(string message) => Add(LogSeverity.Warning, message);
        public ActivityLogEntry Error(string message) => Add(LogSeverity.Error, message);

        public void Clear()
        {
            lock (_gate)
            {
                _entries.Clear();
            }
        }
    }
}
