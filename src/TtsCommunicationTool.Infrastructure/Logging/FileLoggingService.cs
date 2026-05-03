using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Logging;

/// <summary>
/// Diagnostic logging service with two complementary output paths:
///
///   1. Always-on crash/error fallback (plain text via Serilog, WARN and above).
///      Active regardless of user settings — ensures catastrophic failures are always captured.
///
///   2. Per-session structured JSONL log (System.Text.Json, one file per app run).
///      Only active when <see cref="DiagnosticLoggingSettings.EnableDiagnosticLogging"/> is true.
///      Controlled at runtime via <see cref="UpdateSettings"/>.
///
/// Log files are stored at %AppData%\TtsCommunicationTool\logs\.
/// </summary>
public sealed class FileLoggingService : ILoggingService, IDisposable
{
    private static readonly string _logDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool", "logs");

    private static readonly JsonSerializerOptions _metaOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _sessionId;
    private readonly string _sessionFilePath;
    private readonly Serilog.Core.Logger _crashLog;

    private StreamWriter? _sessionWriter;
    private readonly object _writeLock = new();
    private DiagnosticLoggingSettings _settings = new();

    // ── ILoggingService path helpers ─────────────────────────────────────────
    public string LogDirectoryPath => _logDir;
    public string CurrentSessionFilePath => _sessionFilePath;

    // ── ILoggingService metadata read-back ───────────────────────────────────
    public bool LogRawText => _settings.LogRawText && _settings.EnableDiagnosticLogging;
    public bool IncludeRequestIds => _settings.IncludeRequestIds && _settings.EnableDiagnosticLogging;

    // ── Construction ─────────────────────────────────────────────────────────
    public FileLoggingService()
    {
        _sessionId = GenerateSessionId();
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        Directory.CreateDirectory(_logDir);
        _sessionFilePath = Path.Combine(_logDir, $"session_{timestamp}_{_sessionId}.jsonl");

        // Always-on crash fallback — Serilog plain-text, WARN and above, never rolls
        _crashLog = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.File(
                Path.Combine(_logDir, "crash.log"),
                rollingInterval: Serilog.RollingInterval.Infinite,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC [{Level:u5}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    // ── ILoggingService: unstructured legacy methods ──────────────────────────

    public void Debug(string message) => LogLine(DiagnosticLogLevel.Debug, message);

    public void Info(string message) => LogLine(DiagnosticLogLevel.Info, message);

    public void Warn(string message)
    {
        _crashLog.Warning(message);
        LogLine(DiagnosticLogLevel.Warn, message);
    }

    public void Error(string message, Exception? ex = null)
    {
        _crashLog.Error(ex, message);
        LogLine(DiagnosticLogLevel.Error, message,
            ex is null ? null : (object)new { error = ex.Message, stack = ex.StackTrace });
    }

    public void Fatal(string message, Exception? ex = null)
    {
        _crashLog.Fatal(ex, message);
        LogLine(DiagnosticLogLevel.Fatal, message,
            ex is null ? null : (object)new { error = ex.Message, stack = ex.StackTrace });
    }

    // ── ILoggingService: structured logging ──────────────────────────────────

    public void LogEvent(DiagnosticLogLevel level, string category, string eventName, string message, object? metadata = null)
    {
        // Fatal/Error always go to the crash fallback regardless of diagnostic settings
        if (level == DiagnosticLogLevel.Fatal) _crashLog.Fatal(message);
        else if (level == DiagnosticLogLevel.Error) _crashLog.Error(message);

        lock (_writeLock)
        {
            if (_sessionWriter is null || !ShouldLog(level)) return;
            WriteJsonlLocked(level, category, eventName, message, metadata);
        }
    }

    // ── ILoggingService: configuration ───────────────────────────────────────

    public void UpdateSettings(DiagnosticLoggingSettings settings)
    {
        lock (_writeLock)
        {
            _settings = settings;

            if (settings.EnableDiagnosticLogging && _sessionWriter is null)
            {
                _sessionWriter = new StreamWriter(_sessionFilePath, append: true, Encoding.UTF8) { AutoFlush = true };
                WriteJsonlLocked(DiagnosticLogLevel.Info, "app", "logging_started",
                    "Diagnostic session log opened",
                    new { session_id = _sessionId });
            }
            else if (!settings.EnableDiagnosticLogging && _sessionWriter is not null)
            {
                WriteJsonlLocked(DiagnosticLogLevel.Info, "app", "logging_stopped",
                    "Diagnostic session log closed");
                _sessionWriter.Dispose();
                _sessionWriter = null;
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Routes an unstructured legacy log message to the session log if enabled.</summary>
    private void LogLine(DiagnosticLogLevel level, string message, object? metadata = null)
    {
        lock (_writeLock)
        {
            if (_sessionWriter is null || !ShouldLog(level)) return;
            WriteJsonlLocked(level, "app", "log", message, metadata);
        }
    }

    private bool ShouldLog(DiagnosticLogLevel level)
    {
        if (!_settings.EnableDiagnosticLogging) return false;
        if (_settings.TraceLogging)  return true;                                  // TRACE and above
        if (_settings.VerboseLogging) return level >= DiagnosticLogLevel.Debug;    // DEBUG and above
        return level >= DiagnosticLogLevel.Info;                                   // INFO  and above
    }

    /// <summary>
    /// Serialises one JSONL entry to the session writer.
    /// MUST be called with <c>_writeLock</c> held.
    /// </summary>
    private void WriteJsonlLocked(DiagnosticLogLevel level, string category, string eventName, string message, object? metadata = null)
    {
        try
        {
            using var ms = new MemoryStream(512);
            using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { SkipValidation = true });

            writer.WriteStartObject();
            writer.WriteString("ts",         DateTime.UtcNow.ToString("o")); // RFC 3339 / ISO 8601 UTC
            writer.WriteString("level",      level.ToString().ToUpperInvariant());
            writer.WriteString("category",   category);
            writer.WriteString("event",      eventName);
            writer.WriteString("msg",        message);
            writer.WriteString("session_id", _sessionId);

            // Merge optional metadata properties into the top-level object
            if (metadata is not null)
            {
                var metaJson = JsonSerializer.Serialize(metadata, _metaOptions);
                using var metaDoc = JsonDocument.Parse(metaJson);
                foreach (var prop in metaDoc.RootElement.EnumerateObject())
                    prop.WriteTo(writer);
            }

            writer.WriteEndObject();
            writer.Flush();
            _sessionWriter!.WriteLine(Encoding.UTF8.GetString(ms.ToArray()));
        }
        catch
        {
            // Logging must never throw or disrupt the caller
        }
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    /// <summary>Generates a short random hex session identifier (16 chars).</summary>
    private static string GenerateSessionId()
    {
        var bytes = RandomNumberGenerator.GetBytes(8);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Computes a short privacy-safe hash of text for use in log entries.
    /// Returns the first 16 hex characters of SHA-256 (64-bit prefix) — unsuitable for security
    /// but sufficient to correlate requests without exposing the actual content.
    /// </summary>
    public static string ComputeTextHash(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return $"sha256:{Convert.ToHexString(hash)[..16].ToLowerInvariant()}";
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        lock (_writeLock)
        {
            if (_sessionWriter is not null)
            {
                try
                {
                    WriteJsonlLocked(DiagnosticLogLevel.Info, "app", "logging_disposed",
                        "Logger disposed, session log closed");
                }
                catch { /* best-effort */ }
                _sessionWriter.Dispose();
                _sessionWriter = null;
            }
        }
        _crashLog.Dispose();
    }
}
