using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class GeneralSettingsViewModel : ViewModelBase
{
    private static readonly string TranscriptPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool",
        "transcript.txt");

    private readonly ILoggingService _log;

    // ── General ───────────────────────────────────────────────────────────────
    private bool _startWithWindows;
    private bool _showNotifications;
    private bool _showSplashScreen;
    private bool _enableTranscriptLogging;
    private bool _keepOverlayText;
    private bool _enableCharacterLimit = true;
    private int _maxOverlayInputLength = 500;
    private bool _showPlaybackTimer = true;

    // ── Diagnostic Logging ────────────────────────────────────────────────────
    private bool _enableDiagnosticLogging;
    private bool _verboseLogging;
    private bool _traceLogging;
    private bool _includeRequestIds;
    private bool _logRawText;

    // ── Properties: General ───────────────────────────────────────────────────

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetField(ref _startWithWindows, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => SetField(ref _showNotifications, value);
    }

    public bool ShowSplashScreen
    {
        get => _showSplashScreen;
        set => SetField(ref _showSplashScreen, value);
    }

    public bool EnableTranscriptLogging
    {
        get => _enableTranscriptLogging;
        set => SetField(ref _enableTranscriptLogging, value);
    }

    public bool KeepOverlayText
    {
        get => _keepOverlayText;
        set => SetField(ref _keepOverlayText, value);
    }

    public bool EnableCharacterLimit
    {
        get => _enableCharacterLimit;
        set => SetField(ref _enableCharacterLimit, value);
    }

    public int MaxOverlayInputLength
    {
        get => _maxOverlayInputLength;
        set => SetField(ref _maxOverlayInputLength, value < 10 ? 10 : value);
    }

    public bool ShowPlaybackTimer
    {
        get => _showPlaybackTimer;
        set => SetField(ref _showPlaybackTimer, value);
    }

    // Window dimensions are persisted independently — not tracked by IsDirty
    public double SettingsWindowWidth  { get; set; } = 680;
    public double SettingsWindowHeight { get; set; } = 540;
    public double ThemePreviewColumnWidth { get; set; } = 300;

    /// <summary>True when the transcript file exists on disk.</summary>
    public bool HasTranscriptFile => File.Exists(TranscriptPath);

    // ── Properties: Diagnostic Logging ───────────────────────────────────────

    public bool EnableDiagnosticLogging
    {
        get => _enableDiagnosticLogging;
        set => SetField(ref _enableDiagnosticLogging, value);
    }

    public bool VerboseLogging
    {
        get => _verboseLogging;
        set => SetField(ref _verboseLogging, value);
    }

    public bool TraceLogging
    {
        get => _traceLogging;
        set => SetField(ref _traceLogging, value);
    }

    public bool IncludeRequestIds
    {
        get => _includeRequestIds;
        set => SetField(ref _includeRequestIds, value);
    }

    public bool LogRawText
    {
        get => _logRawText;
        set => SetField(ref _logRawText, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public ICommand OpenTranscriptCommand { get; }
    public ICommand OpenLogsFolderCommand { get; }
    public ICommand DeleteOldLogsCommand { get; }

    // ── Constructor ───────────────────────────────────────────────────────────

    public GeneralSettingsViewModel(ILoggingService log)
    {
        _log = log;

        OpenTranscriptCommand = new RelayCommand(() =>
        {
            if (!File.Exists(TranscriptPath)) return;
            Process.Start(new ProcessStartInfo { FileName = TranscriptPath, UseShellExecute = true });
        });

        OpenLogsFolderCommand = new RelayCommand(() =>
        {
            var dir = _log.LogDirectoryPath;
            Directory.CreateDirectory(dir);
            Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
        });

        DeleteOldLogsCommand = new RelayCommand(ExecuteDeleteOldLogs);
    }

    private void ExecuteDeleteOldLogs()
    {
        var result = MessageBox.Show(
            "Are you absolutely sure?\n\n" +
            "Log files are small and important for diagnosing bugs. " +
            "Deleting them cannot be undone.\n\n" +
            "The current session log will be preserved.",
            "Delete Old Logs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes) return;

        var logDir = _log.LogDirectoryPath;
        var current = _log.CurrentSessionFilePath;
        var deleted = 0;
        var failed = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(logDir, "session_*.jsonl"))
            {
                if (string.Equals(file, current, StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    File.Delete(file);
                    deleted++;
                }
                catch
                {
                    failed++;
                }
            }

            var msg = deleted == 0
                ? "No old session logs to delete."
                : $"Deleted {deleted} old session log file(s).";
            if (failed > 0) msg += $" {failed} file(s) could not be deleted (in use).";

            MessageBox.Show(msg, "Delete Old Logs", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to enumerate log files: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Load / Apply ──────────────────────────────────────────────────────────

    public void LoadFrom(GeneralSettings s)
    {
        StartWithWindows       = s.StartWithWindows;
        ShowNotifications      = s.ShowNotifications;
        ShowSplashScreen       = s.ShowSplashScreen;
        EnableTranscriptLogging = s.EnableTranscriptLogging;
        KeepOverlayText        = s.KeepOverlayText;
        EnableCharacterLimit   = s.EnableCharacterLimit;
        MaxOverlayInputLength  = s.MaxOverlayInputLength > 0 ? s.MaxOverlayInputLength : 500;
        ShowPlaybackTimer      = s.ShowPlaybackTimer;
        SettingsWindowWidth    = s.SettingsWindowWidth  > 400 ? s.SettingsWindowWidth  : 680;
        SettingsWindowHeight        = s.SettingsWindowHeight > 300 ? s.SettingsWindowHeight : 540;
        ThemePreviewColumnWidth     = s.ThemePreviewColumnWidth > 80 ? s.ThemePreviewColumnWidth : 300;
    }

    public void ApplyTo(GeneralSettings s)
    {
        s.StartWithWindows       = StartWithWindows;
        s.ShowNotifications      = ShowNotifications;
        s.ShowSplashScreen       = ShowSplashScreen;
        s.EnableTranscriptLogging = EnableTranscriptLogging;
        s.KeepOverlayText        = KeepOverlayText;
        s.EnableCharacterLimit   = EnableCharacterLimit;
        s.MaxOverlayInputLength  = MaxOverlayInputLength;
        s.ShowPlaybackTimer      = ShowPlaybackTimer;
        s.SettingsWindowWidth    = SettingsWindowWidth;
        s.SettingsWindowHeight        = SettingsWindowHeight;
        s.ThemePreviewColumnWidth     = ThemePreviewColumnWidth;
    }

    public void LoadDiagnosticSettings(DiagnosticLoggingSettings s)
    {
        EnableDiagnosticLogging = s.EnableDiagnosticLogging;
        VerboseLogging          = s.VerboseLogging;
        TraceLogging            = s.TraceLogging;
        IncludeRequestIds       = s.IncludeRequestIds;
        LogRawText              = s.LogRawText;
    }

    public void ApplyDiagnosticSettings(DiagnosticLoggingSettings s)
    {
        s.EnableDiagnosticLogging = EnableDiagnosticLogging;
        s.VerboseLogging          = VerboseLogging;
        s.TraceLogging            = TraceLogging;
        s.IncludeRequestIds       = IncludeRequestIds;
        s.LogRawText              = LogRawText;
    }
}
