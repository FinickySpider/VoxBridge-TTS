using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.State;
using TtsCommunicationTool.Core.Validation;
using TtsCommunicationTool.Infrastructure.Logging;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class OverlayViewModel : INotifyPropertyChanged
{
    private readonly ITtsService _tts;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly ITextReplacementService _textReplacement;
    private readonly ITranscriptService _transcript;
    private readonly INotificationService _notifications;
    private readonly PlaybackState _playbackState;
    private readonly RecentMessagesState _recentMessages;
    private string _inputText = string.Empty;
    private string _statusText = string.Empty;  // Empty = idle (no noise)
    private StatusSeverity _statusSeverity = StatusSeverity.None;
    private bool _isSending;

    /// <summary>Raised when the user attempts a blocked submission. Triggers the window shake animation.</summary>
    public event Action? ShakeRequested;

    public OverlayViewModel(
        ITtsService tts,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log,
        ITextReplacementService textReplacement,
        ITranscriptService transcript,
        INotificationService notifications,
        PlaybackState playbackState,
        RecentMessagesState recentMessages)
    {
        _tts = tts;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;
        _textReplacement = textReplacement;
        _transcript = transcript;
        _notifications = notifications;
        _playbackState = playbackState;
        _recentMessages = recentMessages;

        SendCommand = new AsyncRelayCommand(SendAsync, () => CanSend);
        StopCommand = new RelayCommand(Stop, () => _playbackState.IsPlaying);
        ClearCommand = new RelayCommand(Clear);

        _audioRouter.PlaybackFinished += (_, _) =>
        {
            _playbackState.Reset();
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                NotifyCanSubmitChanged();
                if (StatusText is "Speaking..." or "Audio is already playing")
                    StatusText = string.Empty;
            });
        };

        // Reflect external playback state changes (e.g. phrase hotkeys) in the status bar
        _playbackState.PropertyChanged += OnPlaybackStateChanged;

        // If the overlay opens while audio is already playing, show status immediately
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
            SetStatus("Speaking...", StatusSeverity.Info);
    }

    public string InputText
    {
        get => _inputText;
        set
        {
            if (SetField(ref _inputText, value))
            {
                OnPropertyChanged(nameof(CharacterCount));
                OnPropertyChanged(nameof(CharacterCountDisplay));
                NotifyCanSubmitChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public StatusSeverity StatusSeverity
    {
        get => _statusSeverity;
        private set => SetField(ref _statusSeverity, value);
    }

    public bool IsSending
    {
        get => _isSending;
        private set
        {
            if (SetField(ref _isSending, value))
                NotifyCanSubmitChanged();
        }
    }

    /// <summary>True when there is text. Does NOT account for playback blocking.</summary>
    public bool CanSend => !IsSending && !string.IsNullOrWhiteSpace(InputText);

    /// <summary>
    /// True when the Send button should be enabled.
    /// False whenever audio is playing, TTS is generating, or input is empty.
    /// This is the property the Send button binds to for its IsEnabled state.
    /// </summary>
    public bool CanSubmit => CanSend && !_playbackState.IsPlaying && !_audioRouter.IsPlaying;

    /// <summary>0 = unlimited (WPF TextBox.MaxLength behaviour). Config-driven.</summary>
    public int MaxLength =>
        _config.CurrentConfig.GeneralSettings.EnableCharacterLimit
            ? _config.CurrentConfig.GeneralSettings.MaxOverlayInputLength
            : 0;

    public int CharacterCount => InputText.Length;

    /// <summary>Shows "n/max" when limited, or just "n" when unlimited.</summary>
    public string CharacterCountDisplay =>
        _config.CurrentConfig.GeneralSettings.EnableCharacterLimit
            ? $"{InputText.Length}/{_config.CurrentConfig.GeneralSettings.MaxOverlayInputLength}"
            : InputText.Length.ToString();

    /// <summary>Last spoken text, or null if nothing has been sent this session.</summary>
    public string? LastMessage => _recentMessages.GetAll().FirstOrDefault();

    /// <summary>True when there is a message available to resend.</summary>
    public bool HasRecentMessage => LastMessage is not null;

    public ICommand SendCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ClearCommand { get; }

    /// <summary>
    /// Returns true if the given key + modifiers match the configured override hotkey.
    /// Used by OverlayWindow to detect Ctrl+Enter (or user-configured equivalent).
    /// </summary>
    public bool MatchesOverrideHotkey(System.Windows.Input.Key key, System.Windows.Input.ModifierKeys modifiers)
    {
        var h = _config.CurrentConfig.HotkeySettings.OverrideHotkey;
        if (h.IsEmpty) return false;
        return key.ToString().Equals(h.Key, StringComparison.OrdinalIgnoreCase)
            && modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control) == h.Ctrl
            && modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt) == h.Alt
            && modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift) == h.Shift;
    }

    private async Task SendAsync()
    {
        // Block if any audio is already playing (phrase or previous send)
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            SetStatus("Wait until speaking finishes to send next message", StatusSeverity.Warning);
            ShakeRequested?.Invoke();
            _log.LogEvent(DiagnosticLogLevel.Info, "ui", "overlay_submit_blocked",
                "Overlay submit blocked — audio already playing",
                new { reason = "audio_playing" });
            return;
        }

        var text = TextValidation.Sanitize(InputText);
        var gs = _config.CurrentConfig.GeneralSettings;
        var (valid, error) = TextValidation.Validate(text, gs.MaxOverlayInputLength, gs.EnableCharacterLimit);
        if (!valid)
        {
            SetStatus(error!, StatusSeverity.Error);
            ShakeRequested?.Invoke();
            _log.LogEvent(DiagnosticLogLevel.Info, "ui", "overlay_submit_blocked",
                "Overlay submit blocked — validation failed",
                new { reason = "validation", error });
            return;
        }

        text = _textReplacement.Apply(text);
        var pitch = Math.Clamp(_config.CurrentConfig.VoiceSettings.GlobalPitch, 0.5f, 2.0f);

        // Generate a request ID for this TTS pipeline run
        var requestId = _log.IncludeRequestIds
            ? System.Security.Cryptography.RandomNumberGenerator.GetBytes(5)
                  .Aggregate(new System.Text.StringBuilder(), (sb, b) => sb.Append(b.ToString("x2")), sb => sb.ToString())
            : null;

        IsSending = true;
        SetStatus("Generating...", StatusSeverity.Info);
        _log.Info($"Sending text: {text}");

        _log.LogEvent(DiagnosticLogLevel.Info, "tts", "synthesis_requested",
            "TTS synthesis requested from overlay",
            new
            {
                voice_id    = _config.CurrentConfig.VoiceSettings.SelectedVoiceId,
                text_length = text.Length,
                text_hash   = Infrastructure.Logging.FileLoggingService.ComputeTextHash(text),
                text        = _log.LogRawText ? text : (string?)null,
                source      = "overlay",
                request_id  = requestId,
                pitch
            });

        try
        {
            var result = await _tts.SynthesizeAsync(new TtsRequest
            {
                Text      = text,
                VoiceId   = _config.CurrentConfig.VoiceSettings.SelectedVoiceId,
                RequestId = requestId,
                Pitch     = pitch
            });
            if (!result.Success)
            {
                SetStatus($"TTS error: {result.ErrorMessage}", StatusSeverity.Error);
                _notifications.ShowError($"TTS failed: {result.ErrorMessage}");
                _log.Error($"TTS failed: {result.ErrorMessage}");
                _log.LogEvent(DiagnosticLogLevel.Error, "tts", "synthesis_failed",
                    "TTS synthesis failed",
                    new { error = result.ErrorMessage, request_id = requestId });
                return;
            }

            SetStatus("Speaking...", StatusSeverity.Info);
            _playbackState.IsPlaying = true;
            _playbackState.CurrentText = text;
            _recentMessages.Add(text);
            OnPropertyChanged(nameof(LastMessage));
            OnPropertyChanged(nameof(HasRecentMessage));
            _ = _transcript.LogAsync(text);

            _log.LogEvent(DiagnosticLogLevel.Info, "audio", "playback_started",
                "Audio playback started",
                new { source = "overlay", request_id = requestId });

            var cfg = _config.CurrentConfig;
            var playback = new PlaybackRequest
            {
                AudioData    = result.AudioData!,
                SampleRate   = result.SampleRate,
                Channels     = result.Channels,
                BitsPerSample = result.BitsPerSample
            };

            await _audioRouter.PlayAsync(playback,
                cfg.AudioSettings.MonitorOutputDeviceId,
                cfg.AudioSettings.SecondaryOutputDeviceId,
                cfg.AudioSettings.MonitorVolume,
                cfg.AudioSettings.SecondaryVolume);
            InputText = string.Empty;
        }
        catch (Exception ex)
        {
            SetStatus("Error — see log.", StatusSeverity.Error);
            _notifications.ShowError("Playback error occurred.");
            _log.Error("Send failed", ex);
        }
        finally
        {
            IsSending = false;
        }
    }

    /// <summary>
    /// Captures the current text and starts TTS generation + playback asynchronously.
    /// The overlay can close immediately after calling this.
    /// Returns false if submission was blocked (audio already playing / validation failed / empty text).
    /// </summary>
    public bool FireAndForgetSend()
    {
        // Block if any audio is already playing (phrase or previous send)
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            SetStatus("Wait until speaking finishes to send next message", StatusSeverity.Warning);
            ShakeRequested?.Invoke();
            _log.Debug("FireAndForgetSend blocked — audio already playing.");
            return false;
        }

        var text = TextValidation.Sanitize(InputText);
        var gs2 = _config.CurrentConfig.GeneralSettings;
        var (valid2, _) = TextValidation.Validate(text, gs2.MaxOverlayInputLength, gs2.EnableCharacterLimit);
        if (!valid2)
        {
            ShakeRequested?.Invoke();
            return false;
        }

        text = _textReplacement.Apply(text);
        var pitch = Math.Clamp(_config.CurrentConfig.VoiceSettings.GlobalPitch, 0.5f, 2.0f);

        InputText = string.Empty;
        _log.Info($"Fire-and-forget sending text: {text}");

        // Mark playing BEFORE Task.Run so the overlay coordinator sees it immediately
        // and blocks re-open during the TTS generation + playback window.
        _playbackState.IsPlaying = true;
        _playbackState.CurrentText = text;
        _recentMessages.Add(text);
        OnPropertyChanged(nameof(LastMessage));
        OnPropertyChanged(nameof(HasRecentMessage));
        _ = _transcript.LogAsync(text);

        _ = Task.Run(async () =>
        {
            try
            {
                var result = await _tts.SynthesizeAsync(new TtsRequest
                {
                    Text    = text,
                    VoiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId,
                    Pitch   = pitch
                });
                if (!result.Success)
                {
                    _log.Error($"TTS failed: {result.ErrorMessage}");
                    _playbackState.Reset();
                    System.Windows.Application.Current?.Dispatcher.Invoke(
                        () => _notifications.ShowError($"TTS error: {result.ErrorMessage}"));
                    return;
                }

                var cfg = _config.CurrentConfig;
                var playback = new PlaybackRequest
                {
                    AudioData    = result.AudioData!,
                    SampleRate   = result.SampleRate,
                    Channels     = result.Channels,
                    BitsPerSample = result.BitsPerSample
                };

                await _audioRouter.PlayAsync(playback,
                    cfg.AudioSettings.MonitorOutputDeviceId,
                    cfg.AudioSettings.SecondaryOutputDeviceId,
                    cfg.AudioSettings.MonitorVolume,
                    cfg.AudioSettings.SecondaryVolume);
            }
            catch (Exception ex)
            {
                _log.Error("Fire-and-forget send failed", ex);
            }
        });

        return true;
    }

    /// <summary>
    /// Immediately stops any current audio and sends the current overlay text.
    /// Implements the override hotkey (Ctrl+Enter) behaviour.
    /// Returns false if there is no text to send.
    /// </summary>
    public bool ForceStopAndSend()
    {
        var text = TextValidation.Sanitize(InputText);
        if (string.IsNullOrWhiteSpace(text))
        {
            SetStatus("Nothing to send", StatusSeverity.Warning);
            ShakeRequested?.Invoke();
            return false;
        }

        // Stop whatever is currently playing
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            _audioRouter.StopAll();
            _playbackState.Reset();
            _log.Info("Override hotkey: stopped current audio before sending.");
        }

        // Now submit normally — playback state should be clear
        return FireAndForgetSend();
    }

    private void Stop()
    {
        _audioRouter.StopAll();
        _playbackState.Reset();
        SetStatus(string.Empty, StatusSeverity.None);
        _log.Info("Playback stopped by user.");
        _log.LogEvent(DiagnosticLogLevel.Info, "audio", "playback_stopped",
            "Playback stopped by user");
    }

    private void Clear()
    {
        InputText = string.Empty;
        SetStatus(string.Empty, StatusSeverity.None);
    }

    private void SetStatus(string text, StatusSeverity severity = StatusSeverity.None)
    {
        StatusText = text;
        StatusSeverity = severity;
    }

    private void NotifyCanSubmitChanged()
    {
        OnPropertyChanged(nameof(CanSend));
        OnPropertyChanged(nameof(CanSubmit));
        CommandManager.InvalidateRequerySuggested();
    }

    private void OnPlaybackStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PlaybackState.IsPlaying)) return;
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            NotifyCanSubmitChanged();

            // Only set "Speaking..." from external sources (e.g. phrase hotkeys).
            // SendAsync already manages its own "Generating..." / "Speaking..." flow.
            if (_playbackState.IsPlaying && string.IsNullOrEmpty(StatusText))
                SetStatus("Speaking...", StatusSeverity.Info);
            else if (!_playbackState.IsPlaying && StatusText is "Speaking..." or "Audio is already playing")
                SetStatus(string.Empty, StatusSeverity.None);
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
