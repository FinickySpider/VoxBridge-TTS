using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class PhraseListViewModel : ViewModelBase
{
    private readonly IPhraseService _phraseService;
    private readonly IPhraseCacheService _phraseCache;
    private readonly ITtsService _tts;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly IHotkeyHost _hotkeyHost;
    private PhraseItem? _selectedPhrase;

    public ObservableCollection<PhraseItem> Phrases { get; } = new();

    public PhraseItem? SelectedPhrase
    {
        get => _selectedPhrase;
        set
        {
            if (SetField(ref _selectedPhrase, value))
                OnPropertyChanged(nameof(SelectedPhraseHotkeyDisplay));
        }
    }

    public string SelectedPhraseHotkeyDisplay
    {
        get
        {
            if (SelectedPhrase?.Hotkey is not { } hk)
                return "(none)";

            var parts = new List<string>();
            if (hk.Ctrl) parts.Add("Ctrl");
            if (hk.Alt) parts.Add("Alt");
            if (hk.Shift) parts.Add("Shift");
            if (hk.Win) parts.Add("Win");
            if (!string.IsNullOrEmpty(hk.Key)) parts.Add(hk.Key);
            return parts.Count > 0 ? string.Join("+", parts) : "(none)";
        }
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearHotkeyCommand { get; }

    // For inline editing
    private string _editName = string.Empty;
    private string _editText = string.Empty;

    public string EditName
    {
        get => _editName;
        set => SetField(ref _editName, value);
    }

    public string EditText
    {
        get => _editText;
        set => SetField(ref _editText, value);
    }

    public PhraseListViewModel(
        IPhraseService phraseService,
        IPhraseCacheService phraseCache,
        ITtsService tts,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log,
        IHotkeyHost hotkeyHost)
    {
        _phraseService = phraseService;
        _phraseCache = phraseCache;
        _tts = tts;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;
        _hotkeyHost = hotkeyHost;

        AddCommand = new RelayCommand(AddPhrase);
        DeleteCommand = new RelayCommand(DeleteSelected, () => SelectedPhrase is not null);
        PlayCommand = new AsyncRelayCommand(PlaySelectedAsync, () => SelectedPhrase is not null);
        RefreshCommand = new RelayCommand(Refresh);
        ClearHotkeyCommand = new RelayCommand(ClearSelectedHotkey, () => SelectedPhrase is not null);

        Refresh();
    }

    public void Refresh()
    {
        Phrases.Clear();
        foreach (var p in _phraseService.GetAll())
            Phrases.Add(p);
    }

    private void AddPhrase()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditText))
            return;

        var phrase = new PhraseItem
        {
            Name = EditName.Trim(),
            Text = EditText.Trim(),
            SortOrder = Phrases.Count
        };

        var result = _phraseService.Add(phrase);
        if (result.Success)
        {
            EditName = string.Empty;
            EditText = string.Empty;
            Refresh();
        }
    }

    private void DeleteSelected()
    {
        if (SelectedPhrase is null) return;
        var result = _phraseService.Delete(SelectedPhrase.Id);
        if (result.Success)
            Refresh();
    }

    public void SetSelectedPhraseHotkey(HotkeyBinding binding)
    {
        if (SelectedPhrase is null) return;
        SelectedPhrase.Hotkey = binding;
        SelectedPhrase.UpdatedUtc = DateTime.UtcNow;
        _phraseService.Update(SelectedPhrase);
        _hotkeyHost.RegisterPhraseHotkeys();
        OnPropertyChanged(nameof(SelectedPhraseHotkeyDisplay));
        Refresh();
    }

    private void ClearSelectedHotkey()
    {
        if (SelectedPhrase is null) return;
        SelectedPhrase.Hotkey = null;
        SelectedPhrase.UpdatedUtc = DateTime.UtcNow;
        _phraseService.Update(SelectedPhrase);
        _hotkeyHost.RegisterPhraseHotkeys();
        OnPropertyChanged(nameof(SelectedPhraseHotkeyDisplay));
        Refresh();
    }

    private async Task PlaySelectedAsync()
    {
        if (SelectedPhrase is null) return;

        var cfg = _config.CurrentConfig;

        // Try cached audio first
        var cached = _phraseCache.GetCachedAudio(SelectedPhrase.Id);
        if (cached is not null)
        {
            await _audioRouter.PlayAsync(cached,
                cfg.AudioSettings.MonitorOutputDeviceId,
                cfg.AudioSettings.SecondaryOutputDeviceId);
            return;
        }

        // Fallback: generate on-the-fly and cache it
        var ttsResult = await _tts.SynthesizeAsync(new TtsRequest
        {
            Text = SelectedPhrase.Text,
            VoiceId = cfg.VoiceSettings.SelectedVoiceId
        });

        if (!ttsResult.Success || ttsResult.AudioData is null)
        {
            _log.Warn($"Phrase playback failed: {ttsResult.ErrorMessage}");
            return;
        }

        // Cache for next time
        _ = _phraseCache.GenerateCacheAsync(SelectedPhrase);

        var playback = new PlaybackRequest
        {
            AudioData = ttsResult.AudioData,
            SampleRate = ttsResult.SampleRate,
            Channels = ttsResult.Channels,
            BitsPerSample = ttsResult.BitsPerSample
        };

        await _audioRouter.PlayAsync(playback,
            cfg.AudioSettings.MonitorOutputDeviceId,
            cfg.AudioSettings.SecondaryOutputDeviceId);
    }
}
