using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Logging
{
    // Very small file logger implementation used by the app for diagnostics.
    // This intentionally keeps behaviour minimal and synchronous so it's
    // easy to reason about for debugging and CI tests. It writes plain text
    // log lines into a single file under build/diagnostics.
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly StreamWriter _writer;
        private readonly object _lock = new object();
        public LogLevel MinLevel { get; }

        public FileLoggerProvider(string filePath, LogLevel minLevel = LogLevel.Information)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            // Ensure directory exists
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // Open the file for append so we preserve any existing logs.
                // Older behaviour observed truncating the file on startup in some
                // environments; using OpenOrCreate + Seek ensures we never overwrite
                // existing contents and have a chance to append a session header.
                bool existed = File.Exists(filePath);
                var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                // Move to end so we append
                fs.Seek(0, SeekOrigin.End);
                _writer = new StreamWriter(fs) { AutoFlush = true };

                // Always write a session separator so different runs are obvious
                try
                {
                    var header = $"\n=== NEW SESSION: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fffZ} ===\n";
                    _writer.WriteLine(header);
                }
                catch { /* best-effort */ }
            }
            catch (Exception ex)
            {
                // If creation fails, surface a friendly exception quickly instead of failing silently.
                throw new InvalidOperationException($"Failed to create log file '{filePath}'", ex);
            }

            MinLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);

        public void Dispose()
        {
            lock (_lock)
            {
                try { _writer?.Dispose(); } catch { }
            }
        }

        private sealed class FileLogger : ILogger
        {
            private readonly string _category;
            private readonly FileLoggerProvider _provider;

            public FileLogger(string category, FileLoggerProvider provider)
            {
                _category = category ?? "<unknown>";
                _provider = provider;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance; // scopes not supported

            public bool IsEnabled(LogLevel logLevel) => logLevel >= _provider.MinLevel;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;
                if (formatter == null) return;

                var msg = formatter(state, exception);
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffZ");
                var level = logLevel.ToString().ToUpperInvariant();

                var line = $"[{timestamp}] {level} { _category }[{eventId.Id}] - {msg}";
                if (exception != null)
                {
                    line += $"\n{exception}\n";
                }

                lock (_provider._lock)
                {
                    try { _provider._writer.WriteLine(line); } catch { /* best-effort logging */ }
                }
            }

            // Very tiny disposable used to satisfy BeginScope semantics; scopes not used here
            private sealed class NoopDisposable : IDisposable
            {
                public static readonly NoopDisposable Instance = new NoopDisposable();
                public void Dispose() { }
            }
        }
    }
}
