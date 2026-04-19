using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class PhraseListViewModel : ViewModelBase
{
    private readonly IPhraseService _phraseService;
    private readonly ITtsService _tts;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private PhraseItem? _selectedPhrase;

    public ObservableCollection<PhraseItem> Phrases { get; } = new();

    public PhraseItem? SelectedPhrase
    {
        get => _selectedPhrase;
        set => SetField(ref _selectedPhrase, value);
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand RefreshCommand { get; }

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
        ITtsService tts,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log)
    {
        _phraseService = phraseService;
        _tts = tts;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;

        AddCommand = new RelayCommand(AddPhrase);
        DeleteCommand = new RelayCommand(DeleteSelected, () => SelectedPhrase is not null);
        PlayCommand = new AsyncRelayCommand(PlaySelectedAsync, () => SelectedPhrase is not null);
        RefreshCommand = new RelayCommand(Refresh);

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

    private async Task PlaySelectedAsync()
    {
        if (SelectedPhrase is null) return;

        var ttsResult = await _tts.SynthesizeAsync(new TtsRequest
        {
            Text = SelectedPhrase.Text,
            VoiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId
        });

        if (!ttsResult.Success || ttsResult.AudioData is null)
        {
            _log.Warn($"Phrase playback failed: {ttsResult.ErrorMessage}");
            return;
        }

        var playback = new PlaybackRequest
        {
            AudioData = ttsResult.AudioData,
            SampleRate = ttsResult.SampleRate,
            Channels = ttsResult.Channels,
            BitsPerSample = ttsResult.BitsPerSample
        };

        var cfg = _config.CurrentConfig;
        await _audioRouter.PlayAsync(playback,
            cfg.AudioSettings.MonitorOutputDeviceId,
            cfg.AudioSettings.SecondaryOutputDeviceId);
    }
}
