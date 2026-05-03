using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>Severity level for structured diagnostic log events.</summary>
public enum DiagnosticLogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}

public interface ILoggingService
{
    // ── Unstructured legacy methods (backward-compatible) ─────────────────────
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
    void Fatal(string message, Exception? ex = null);

    // ── Structured diagnostic logging ─────────────────────────────────────────
    /// <summary>
    /// Writes a structured JSONL log entry when diagnostic logging is enabled.
    /// <paramref name="metadata"/> is an anonymous object whose properties are merged
    /// into the top-level log entry as additional fields (snake_case).
    /// </summary>
    void LogEvent(DiagnosticLogLevel level, string category, string eventName, string message, object? metadata = null);

    // ── Configuration ─────────────────────────────────────────────────────────
    /// <summary>Applies updated diagnostic logging settings at runtime without restarting the app.</summary>
    void UpdateSettings(DiagnosticLoggingSettings settings);

    // ── Metadata read-back (for privacy-conditional logging at call sites) ────
    /// <summary>True when raw spoken text may be included in log entries.</summary>
    bool LogRawText { get; }
    /// <summary>True when request correlation IDs should be generated and attached.</summary>
    bool IncludeRequestIds { get; }

    // ── Path helpers (for UI buttons) ─────────────────────────────────────────
    string LogDirectoryPath { get; }
    string CurrentSessionFilePath { get; }
}
