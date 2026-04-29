using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Tts;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class VoiceSettingsViewModel : ViewModelBase
{
    private readonly ITtsService _tts;
    private readonly ElevenLabsTtsService _elevenLabs;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;

    // ── Kokoro ───────────────────────────────────────────────────────────────
    private string _selectedVoiceId = string.Empty;
    private string _engineName = "Kokoro";

    // ── Engine selection ─────────────────────────────────────────────────────
    private VoiceEngine _engine = VoiceEngine.Kokoro;
    public VoiceEngine Engine
    {
        get => _engine;
        set
        {
            if (SetField(ref _engine, value))
            {
                OnPropertyChanged(nameof(IsKokoro));
                OnPropertyChanged(nameof(IsElevenLabs));
                LoadVoices();
            }
        }
    }
    public bool IsKokoro
    {
        get => _engine == VoiceEngine.Kokoro;
        set { if (value) Engine = VoiceEngine.Kokoro; }
    }
    public bool IsElevenLabs
    {
        get => _engine == VoiceEngine.ElevenLabs;
        set { if (value) Engine = VoiceEngine.ElevenLabs; }
    }

    // ── Kokoro props ─────────────────────────────────────────────────────────
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

    // ── ElevenLabs props ─────────────────────────────────────────────────────
    private string _elevenLabsApiKey = string.Empty;
    private string _elevenLabsModelId = "eleven_multilingual_v2";
    private string _elevenLabsSelectedVoiceId = string.Empty;
    private string _elevenLabsSelectedVoiceName = string.Empty;
    private string _elevenLabsStatus = string.Empty;

    public string ElevenLabsApiKey
    {
        get => _elevenLabsApiKey;
        set => SetField(ref _elevenLabsApiKey, value);
    }
    public string ElevenLabsModelId
    {
        get => _elevenLabsModelId;
        set => SetField(ref _elevenLabsModelId, value);
    }
    public string ElevenLabsSelectedVoiceId
    {
        get => _elevenLabsSelectedVoiceId;
        set => SetField(ref _elevenLabsSelectedVoiceId, value);
    }
    public string ElevenLabsSelectedVoiceName
    {
        get => _elevenLabsSelectedVoiceName;
        set => SetField(ref _elevenLabsSelectedVoiceName, value);
    }
    public string ElevenLabsStatus
    {
        get => _elevenLabsStatus;
        set => SetField(ref _elevenLabsStatus, value);
    }
    public ObservableCollection<VoiceInfo> ElevenLabsVoices { get; } = new();

    // ── Commands ─────────────────────────────────────────────────────────────
    public ICommand TestVoiceCommand { get; }
    public ICommand FetchElevenLabsVoicesCommand { get; }

    public VoiceSettingsViewModel(
        ITtsService tts,
        ElevenLabsTtsService elevenLabs,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log)
    {
        _tts = tts;
        _elevenLabs = elevenLabs;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;

        TestVoiceCommand = new AsyncRelayCommand(TestVoiceAsync);
        FetchElevenLabsVoicesCommand = new AsyncRelayCommand(FetchElevenLabsVoicesAsync);
        LoadVoices();
    }

    private void LoadVoices()
    {
        AvailableVoices.Clear();
        if (_engine == VoiceEngine.ElevenLabs)
        {
            foreach (var v in _elevenLabs.GetAvailableVoices())
                AvailableVoices.Add(v);
        }
        else
        {
            foreach (var v in _tts.GetAvailableVoices())
                AvailableVoices.Add(v);
        }
    }

    private async Task FetchElevenLabsVoicesAsync()
    {
        ElevenLabsStatus = "Fetching voices…";
        // Temporarily persist the API key so the service can use it
        _config.CurrentConfig.ElevenLabs.ApiKey = ElevenLabsApiKey;

        var voices = await _elevenLabs.FetchVoicesAsync();
        ElevenLabsVoices.Clear();
        foreach (var v in voices)
            ElevenLabsVoices.Add(v);

        if (voices.Count > 0)
        {
            ElevenLabsStatus = $"Loaded {voices.Count} voices.";
            // Restore selected voice if still valid
            if (ElevenLabsVoices.All(v => v.Id != ElevenLabsSelectedVoiceId) && voices.Count > 0)
            {
                ElevenLabsSelectedVoiceId = voices[0].Id;
                ElevenLabsSelectedVoiceName = voices[0].DisplayName;
            }
            // Also update AvailableVoices if engine is ElevenLabs
            if (_engine == VoiceEngine.ElevenLabs)
                LoadVoices();
        }
        else
        {
            ElevenLabsStatus = "No voices returned — check API key.";
        }
    }

    private async Task TestVoiceAsync()
    {
        string voiceId = _engine == VoiceEngine.ElevenLabs ? ElevenLabsSelectedVoiceId : SelectedVoiceId;
        if (string.IsNullOrEmpty(voiceId)) return;

        // Temporarily apply settings so the active service has the right values
        ApplyToConfig();

        var result = await _tts.SynthesizeAsync(new TtsRequest
        {
            Text = "Hello, this is a voice test.",
            VoiceId = voiceId
        });

        if (!result.Success || result.AudioData is null)
        {
            _log.Warn($"Voice test failed: {result.ErrorMessage}");
            if (_engine == VoiceEngine.ElevenLabs)
                ElevenLabsStatus = $"Test failed: {result.ErrorMessage}";
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
        if (_engine == VoiceEngine.ElevenLabs)
            ElevenLabsStatus = "Test playback started.";
    }

    public void LoadFrom(VoiceSettings s)
    {
        LoadVoices();
        SelectedVoiceId = s.SelectedVoiceId;
        EngineName = s.EngineName;
        _engine = s.Engine;   // set backing field directly to avoid double LoadVoices
        OnPropertyChanged(nameof(Engine));
        OnPropertyChanged(nameof(IsKokoro));
        OnPropertyChanged(nameof(IsElevenLabs));

        var el = _config.CurrentConfig.ElevenLabs;
        ElevenLabsApiKey = el.ApiKey;
        ElevenLabsModelId = el.ModelId;
        ElevenLabsSelectedVoiceId = el.SelectedVoiceId;
        ElevenLabsSelectedVoiceName = el.SelectedVoiceName;

        // Load any previously cached ElevenLabs voices
        ElevenLabsVoices.Clear();
        foreach (var v in _elevenLabs.GetAvailableVoices())
            ElevenLabsVoices.Add(v);

        LoadVoices();
    }

    public void ApplyTo(VoiceSettings s)
    {
        s.Engine = _engine;
        s.EngineName = _engine.ToString();
        s.SelectedVoiceId = SelectedVoiceId;
        s.SelectedVoiceDisplayName = AvailableVoices.FirstOrDefault(v => v.Id == SelectedVoiceId)?.DisplayName ?? string.Empty;
        ApplyToConfig();
    }

    /// <summary>Persists ElevenLabs settings into live config without a full save.</summary>
    private void ApplyToConfig()
    {
        var el = _config.CurrentConfig.ElevenLabs;
        el.ApiKey = ElevenLabsApiKey;
        el.ModelId = ElevenLabsModelId;
        el.SelectedVoiceId = ElevenLabsSelectedVoiceId;
        el.SelectedVoiceName = ElevenLabsSelectedVoiceName;
    }
}

