using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class VoiceSettingsViewModel : ViewModelBase
{
    private readonly ITtsService _tts;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private string _selectedVoiceId = string.Empty;
    private string _engineName = "Kokoro";

    public ObservableCollection<VoiceInfo> AvailableVoices { get; } = new();

    public string SelectedVoiceId
    {
        get => _selectedVoiceId;
        set => SetField(ref _selectedVoiceId, value);
    }

    public string EngineName
    {
        get => _engineName;
        set => SetField(ref _engineName, value);
    }

    public ICommand TestVoiceCommand { get; }

    public VoiceSettingsViewModel(
        ITtsService tts,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log)
    {
        _tts = tts;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;

        TestVoiceCommand = new AsyncRelayCommand(TestVoiceAsync);
        LoadVoices();
    }

    private void LoadVoices()
    {
        AvailableVoices.Clear();
        foreach (var voice in _tts.GetAvailableVoices())
            AvailableVoices.Add(voice);
    }

    private async Task TestVoiceAsync()
    {
        if (string.IsNullOrEmpty(SelectedVoiceId)) return;

        var result = await _tts.SynthesizeAsync(new TtsRequest
        {
            Text = "Hello, this is a voice test.",
            VoiceId = SelectedVoiceId
        });

        if (!result.Success || result.AudioData is null)
        {
            _log.Warn($"Voice test failed: {result.ErrorMessage}");
            return;
        }

        var playback = new PlaybackRequest
        {
            AudioData = result.AudioData,
            SampleRate = result.SampleRate,
            Channels = result.Channels,
            BitsPerSample = result.BitsPerSample
        };

        var cfg = _config.CurrentConfig;
        await _audioRouter.PlayAsync(playback, cfg.AudioSettings.MonitorOutputDeviceId, null);
    }

    public void LoadFrom(VoiceSettings s)
    {
        // Reload voice list in case TTS was initialized after construction
        LoadVoices();
        SelectedVoiceId = s.SelectedVoiceId;
        EngineName = s.EngineName;
    }

    public void ApplyTo(VoiceSettings s)
    {
        s.SelectedVoiceId = SelectedVoiceId;
        s.EngineName = EngineName;
    }
}
