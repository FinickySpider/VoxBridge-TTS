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

public sealed class OverlayViewModel : INotifyPropertyChanged, IDisposable
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

    // ── Playback countdown timer ────────────────────────────────────────────────
    private bool     _speakingStatusActive;
    private double   _playbackDurationSeconds;
    private DateTime _playbackStartUtc;
    private System.Windows.Threading.DispatcherTimer? _countdownTimer;
    // Stored so Dispose() can cleanly unsubscribe
    private readonly EventHandler _playbackFinishedHandler;

    // ── Status hold: prevents countdown ticks from overwriting transient warning messages ──
    // Set to UtcNow + 1.5s whenever a Warning/Error status is shown mid-playback.
    private DateTime _statusBlockedUntil = DateTime.MinValue;

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

        _playbackFinishedHandler = (_, _) =>
        {
            _playbackState.Reset();
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                StopSpeaking();
                NotifyCanSubmitChanged();
            });
        };
        _audioRouter.PlaybackFinished += _playbackFinishedHandler;

        // Reflect external playback state changes (e.g. phrase hotkeys) in the status bar
        _playbackState.PropertyChanged += OnPlaybackStateChanged;

        // If the overlay opens while audio is already playing, resume countdown from shared state.
        // PlaybackDurationSeconds may still be 0 here if synthesis hasn't finished yet —
        // OnPlaybackStateChanged will upgrade us to a live countdown once it arrives.
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            var dur  = _playbackState.PlaybackDurationSeconds;
            var when = _playbackState.PlaybackStartedUtc;
            StartSpeaking(dur > 0 && when != default ? dur : 0,
                          dur > 0 && when != default ? when : DateTime.UtcNow);
        }
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

            // Apply silence trimming then compute duration for the countdown timer
            var audioData = ApplySilenceTrim(result.AudioData!, result.SampleRate, result.Channels, result.BitsPerSample);
            double dur = audioData.Length / (double)(result.SampleRate * result.Channels * (result.BitsPerSample / 8));

            _playbackState.IsPlaying = true;
            _playbackState.CurrentText = text;
            _recentMessages.Add(text);
            OnPropertyChanged(nameof(LastMessage));
            OnPropertyChanged(nameof(HasRecentMessage));
            _ = _transcript.LogAsync(text);

            StartSpeaking(dur); // sets "Speaking… (Xs)" status and starts timer

            _log.LogEvent(DiagnosticLogLevel.Info, "audio", "playback_started",
                "Audio playback started",
                new { source = "overlay", request_id = requestId });

            var cfg = _config.CurrentConfig;
            var playback = new PlaybackRequest
            {
                AudioData    = audioData,
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

                var audioData = ApplySilenceTrim(result.AudioData!, result.SampleRate, result.Channels, result.BitsPerSample);
                double dur = audioData.Length / (double)(result.SampleRate * result.Channels * (result.BitsPerSample / 8));
                // Write to singleton BEFORE PlayAsync so any open overlay can start the countdown.
                // Do NOT dispatch StartSpeaking — this VM is disposed (overlay closed on send).
                // The open overlay (if any) listens on PlaybackState.PropertyChanged and upgrades.
                _playbackState.PlaybackStartedUtc      = DateTime.UtcNow;
                _playbackState.PlaybackDurationSeconds = dur;

                var cfg = _config.CurrentConfig;
                var playback = new PlaybackRequest
                {
                    AudioData    = audioData,
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
                _playbackState.Reset(); // Ensure CanSubmit recovers if PlayAsync throws
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

        text = _textReplacement.Apply(text);
        var pitch = Math.Clamp(_config.CurrentConfig.VoiceSettings.GlobalPitch, 0.5f, 2.0f);

        // Clear input and log transcript on the UI thread before going async
        InputText = string.Empty;
        _recentMessages.Add(text);
        OnPropertyChanged(nameof(LastMessage));
        OnPropertyChanged(nameof(HasRecentMessage));
        _ = _transcript.LogAsync(text);

        // Mark playing immediately so CanSubmit blocks new sends during stop+synthesis.
        // We do this BEFORE Task.Run so no second send can slip through the gap.
        _playbackState.IsPlaying = true;
        _playbackState.CurrentText = text;
        NotifyCanSubmitChanged();

        _ = Task.Run(async () =>
        {
            try
            {
                // StopAll cancels the in-flight playback CTS so PlaybackFinished does
                // NOT fire for the old audio — eliminating the race that would reset
                // _playbackState.IsPlaying back to false mid-synthesis.
                // Running on the thread pool also prevents blocking the UI thread
                // (WasapiOut.Stop() internally calls playThread.Join()).
                _audioRouter.StopAll();

                _log.Info("Override hotkey: stopped current audio, beginning new synthesis.");

                System.Windows.Application.Current?.Dispatcher.InvokeAsync(
                    () => SetStatus("Generating...", StatusSeverity.Info));

                var result = await _tts.SynthesizeAsync(new TtsRequest
                {
                    Text    = text,
                    VoiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId,
                    Pitch   = pitch
                });

                if (!result.Success)
                {
                    _playbackState.Reset();
                    System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                    {
                        SetStatus("Error — see log.", StatusSeverity.Error);
                        _notifications.ShowError($"TTS error: {result.ErrorMessage}");
                    });
                    _log.Error($"ForceStopAndSend TTS failed: {result.ErrorMessage}");
                    return;
                }

                var audioData = ApplySilenceTrim(result.AudioData!, result.SampleRate, result.Channels, result.BitsPerSample);
                double dur = audioData.Length / (double)(result.SampleRate * result.Channels * (result.BitsPerSample / 8));
                // Write to singleton; the open overlay upgrades via PlaybackState.PropertyChanged.
                _playbackState.PlaybackStartedUtc      = DateTime.UtcNow;
                _playbackState.PlaybackDurationSeconds = dur;

                var cfg = _config.CurrentConfig;
                await _audioRouter.PlayAsync(
                    new PlaybackRequest
                    {
                        AudioData     = audioData,
                        SampleRate    = result.SampleRate,
                        Channels      = result.Channels,
                        BitsPerSample = result.BitsPerSample
                    },
                    cfg.AudioSettings.MonitorOutputDeviceId,
                    cfg.AudioSettings.SecondaryOutputDeviceId,
                    cfg.AudioSettings.MonitorVolume,
                    cfg.AudioSettings.SecondaryVolume);
            }
            catch (Exception ex)
            {
                _playbackState.Reset(); // Always recover CanSubmit on any error
                _log.Error("ForceStopAndSend failed", ex);
            }
        });

        return true;
    }

    // ── Playback timer helpers ────────────────────────────────────────────────────

    private void StartSpeaking(double durationSeconds)
        => StartSpeaking(durationSeconds, DateTime.UtcNow);

    private void StartSpeaking(double durationSeconds, DateTime startUtc)
    {
        _speakingStatusActive    = true;
        _playbackDurationSeconds = durationSeconds;
        _playbackStartUtc        = startUtc;
        // Persist in singleton so the next overlay open can resume the countdown.
        // Only overwrite if we have real data (don't clobber with zeros from phrase-hotkey path).
        if (durationSeconds > 0)
        {
            _playbackState.PlaybackDurationSeconds = durationSeconds;
            _playbackState.PlaybackStartedUtc      = startUtc;
        }

        _countdownTimer?.Stop();
        _countdownTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 fps — smooth sub-second display
        };
        _countdownTimer.Tick += OnCountdownTick;
        _countdownTimer.Start();

        SetStatus(BuildSpeakingStatus(), StatusSeverity.Info);
    }

    private void StopSpeaking()
    {
        _speakingStatusActive = false;
        _countdownTimer?.Stop();
        _countdownTimer = null;
        // NOTE: do NOT clear PlaybackState timing here — it must survive across overlay opens
        // so the next open can resume the countdown. PlaybackState.Reset() clears it when audio ends.

        if (StatusText.StartsWith("Speaking", StringComparison.Ordinal))
            SetStatus(string.Empty, StatusSeverity.None);
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        if (!_speakingStatusActive)
        {
            _countdownTimer?.Stop();
            return;
        }
        // Hold off updating if a transient warning/error message is still being shown.
        if (DateTime.UtcNow < _statusBlockedUntil)
            return;
        StatusText = BuildSpeakingStatus();
    }

    private string BuildSpeakingStatus()
    {
        var showTimer = _config.CurrentConfig.GeneralSettings.ShowPlaybackTimer;
        if (!showTimer || _playbackDurationSeconds <= 0)
            return "Speaking...";

        double elapsed   = (DateTime.UtcNow - _playbackStartUtc).TotalSeconds;
        double remaining = Math.Max(0, _playbackDurationSeconds - elapsed);
        int    mins      = (int)(remaining / 60);
        double secs      = remaining - mins * 60;
        return mins > 0
            ? $"Speaking...  ({mins}m {secs:00.0}s)"
            : $"Speaking...  ({secs:0.0}s)";
    }

    private byte[] ApplySilenceTrim(byte[] audioData, int sampleRate, int channels, int bitsPerSample)
    {
        var audio = _config.CurrentConfig.AudioSettings;
        if (!audio.TrimTrailingSilence)
            return audioData;

        float retention = Math.Clamp(audio.SilenceRetentionPercent / 100.0f, 0.05f, 1.0f);
        return TtsCommunicationTool.Core.Utilities.SilenceTrimmer.Trim(
            audioData, sampleRate, channels, bitsPerSample, retention);
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
        // If audio is playing and we just set a transient warning/error (e.g. "blocked send"),
        // block the countdown ticker from overwriting it for 1.5 seconds so the user can read it.
        if (_speakingStatusActive && severity is StatusSeverity.Warning or StatusSeverity.Error
            && !text.StartsWith("Speaking", StringComparison.Ordinal))
        {
            _statusBlockedUntil = DateTime.UtcNow.AddSeconds(1.5);
        }
    }

    private void NotifyCanSubmitChanged()
    {
        OnPropertyChanged(nameof(CanSend));
        OnPropertyChanged(nameof(CanSubmit));
        CommandManager.InvalidateRequerySuggested();
    }

    private void OnPlaybackStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            // IsPlaying changed
            if (e.PropertyName == nameof(PlaybackState.IsPlaying))
            {
                NotifyCanSubmitChanged();
                if (_playbackState.IsPlaying && string.IsNullOrEmpty(StatusText))
                    StartSpeaking(0); // phrase hotkey path — no duration known yet
                else if (!_playbackState.IsPlaying)
                    StopSpeaking();
                return;
            }

            // Timing data arrived (synthesis just finished while this overlay was already open).
            // Upgrade from static "Speaking..." to a live countdown.
            if (e.PropertyName == nameof(PlaybackState.PlaybackDurationSeconds))
            {
                var dur  = _playbackState.PlaybackDurationSeconds;
                var when = _playbackState.PlaybackStartedUtc;
                if (_speakingStatusActive && _playbackDurationSeconds == 0 && dur > 0 && when != default)
                    StartSpeaking(dur, when);
            }
        });
    }

    public void Dispose()
    {
        // Unsubscribe events so this dead VM can't react after close (and won't prevent GC).
        _audioRouter.PlaybackFinished -= _playbackFinishedHandler;
        _playbackState.PropertyChanged -= OnPlaybackStateChanged;
        // Stop the countdown timer. PlaybackState timing survives — next open resumes from there.
        _countdownTimer?.Stop();
        _countdownTimer = null;
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
