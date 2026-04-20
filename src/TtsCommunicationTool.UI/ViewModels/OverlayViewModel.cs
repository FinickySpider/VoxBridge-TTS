using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.State;
using TtsCommunicationTool.Core.Validation;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class OverlayViewModel : INotifyPropertyChanged
{
    private readonly ITtsService _tts;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly PlaybackState _playbackState;
    private readonly RecentMessagesState _recentMessages;
    private string _inputText = string.Empty;
    private string _statusText = string.Empty;  // Empty = idle (no noise)
    private bool _isSending;

    public OverlayViewModel(
        ITtsService tts,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log,
        PlaybackState playbackState,
        RecentMessagesState recentMessages)
    {
        _tts = tts;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;
        _playbackState = playbackState;
        _recentMessages = recentMessages;

        SendCommand = new AsyncRelayCommand(SendAsync, () => CanSend);
        StopCommand = new RelayCommand(Stop, () => _playbackState.IsPlaying);
        ClearCommand = new RelayCommand(Clear);

        _audioRouter.PlaybackFinished += (_, _) =>
        {
            _playbackState.Reset();
            StatusText = string.Empty;
        };

        // Reflect external playback state changes (e.g. phrase hotkeys) in the status bar
        _playbackState.PropertyChanged += OnPlaybackStateChanged;
    }

    public string InputText
    {
        get => _inputText;
        set
        {
            if (SetField(ref _inputText, value))
            {
                OnPropertyChanged(nameof(CharacterCount));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public bool IsSending
    {
        get => _isSending;
        private set => SetField(ref _isSending, value);
    }

    public bool CanSend => !IsSending && !string.IsNullOrWhiteSpace(InputText);

    public int MaxLength => TextValidation.MaxInputLength;
    public int CharacterCount => InputText.Length;

    public ICommand SendCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ClearCommand { get; }

    private async Task SendAsync()
    {
        // Block if any audio is already playing (phrase or previous send)
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            StatusText = "Audio is still playing...";
            return;
        }

        var text = TextValidation.Sanitize(InputText);
        var (valid, error) = TextValidation.Validate(text);
        if (!valid)
        {
            StatusText = error!;
            return;
        }

        IsSending = true;
        StatusText = "Generating...";
        _log.Info($"Sending text: {text}");

        try
        {
            var result = await _tts.SynthesizeAsync(new TtsRequest
            {
                Text = text,
                VoiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId
            });
            if (!result.Success)
            {
                StatusText = $"TTS error: {result.ErrorMessage}";
                _log.Error($"TTS failed: {result.ErrorMessage}");
                return;
            }

            StatusText = "Speaking...";
            _playbackState.IsPlaying = true;
            _playbackState.CurrentText = text;

            var cfg = _config.CurrentConfig;
            var playback = new PlaybackRequest
            {
                AudioData = result.AudioData!,
                SampleRate = result.SampleRate,
                Channels = result.Channels,
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
            StatusText = "Error — see log.";
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
    /// </summary>
    public void FireAndForgetSend()
    {
        // Block if any audio is already playing (phrase or previous send)
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            _log.Debug("FireAndForgetSend blocked — audio already playing.");
            return;
        }

        var text = TextValidation.Sanitize(InputText);
        var (valid, _) = TextValidation.Validate(text);
        if (!valid) return;

        InputText = string.Empty;
        _log.Info($"Fire-and-forget sending text: {text}");

        // Mark playing BEFORE Task.Run so the overlay coordinator sees it immediately
        // and blocks re-open during the TTS generation + playback window.
        _playbackState.IsPlaying = true;
        _playbackState.CurrentText = text;
        _recentMessages.Add(text);

        _ = Task.Run(async () =>
        {
            try
            {
                var result = await _tts.SynthesizeAsync(new TtsRequest
                {
                    Text = text,
                    VoiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId
                });
                if (!result.Success)
                {
                    _log.Error($"TTS failed: {result.ErrorMessage}");
                    _playbackState.Reset();
                    return;
                }

                var cfg = _config.CurrentConfig;
                var playback = new PlaybackRequest
                {
                    AudioData = result.AudioData!,
                    SampleRate = result.SampleRate,
                    Channels = result.Channels,
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
    }

    private void Stop()
    {
        _audioRouter.StopAll();
        _playbackState.Reset();
        StatusText = string.Empty;
        _log.Info("Playback stopped by user.");
    }

    private void Clear()
    {
        InputText = string.Empty;
        StatusText = string.Empty;
    }

    private void OnPlaybackStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PlaybackState.IsPlaying)) return;
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            // Only set "Speaking..." from external sources (e.g. phrase hotkeys).
            // SendAsync already manages its own "Generating..." / "Speaking..." flow.
            if (_playbackState.IsPlaying && string.IsNullOrEmpty(StatusText))
                StatusText = "Speaking...";
            else if (!_playbackState.IsPlaying && StatusText == "Speaking...")
                StatusText = string.Empty;
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
