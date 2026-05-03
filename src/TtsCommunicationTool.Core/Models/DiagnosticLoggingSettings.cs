namespace TtsCommunicationTool.Core.Models;

/// <summary>Configures structured diagnostic and debug logging behaviour.</summary>
public sealed class DiagnosticLoggingSettings
{
    /// <summary>
    /// When true, structured JSONL session logs are written to disk.
    /// When false, only the always-on crash/fatal fallback log is active.
    /// </summary>
    public bool EnableDiagnosticLogging { get; set; } = false;

    /// <summary>
    /// When true, DEBUG-level events are included in the session log.
    /// Requires <see cref="EnableDiagnosticLogging"/> to be true.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// When true, TRACE-level events are included (very chatty — includes all DEBUG events too).
    /// Requires <see cref="EnableDiagnosticLogging"/> to be true.
    /// </summary>
    public bool TraceLogging { get; set; } = false;

    /// <summary>
    /// When true, a short request_id is attached to TTS pipeline log events to correlate them.
    /// Requires <see cref="EnableDiagnosticLogging"/> to be true.
    /// </summary>
    public bool IncludeRequestIds { get; set; } = false;

    /// <summary>
    /// When true, raw spoken text may be written to log files.
    /// WARNING: this can capture sensitive or private content — leave off by default.
    /// Requires <see cref="EnableDiagnosticLogging"/> to be true.
    /// </summary>
    public bool LogRawText { get; set; } = false;
}
