using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
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
    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }

    // For inline editing
    private string _editName = string.Empty;
    private string _editText = string.Empty;
    private string _phraseStatusMessage = string.Empty;

    public string PhraseStatusMessage
    {
        get => _phraseStatusMessage;
        set => SetField(ref _phraseStatusMessage, value);
    }

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

    /// <summary>Result of the most recent successful import. Used by the import progress dialog.</summary>
    public PhraseImportResult? LastImportResult { get; private set; }

    /// <summary>Raised after a successful import so the view can show the progress dialog.</summary>
    public event EventHandler? ImportCompleted;

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
        ImportCommand = new RelayCommand(ImportPhrases);
        ExportCommand = new RelayCommand(ExportPhrases, () => Phrases.Count > 0);

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
                cfg.AudioSettings.SecondaryOutputDeviceId,
                cfg.AudioSettings.MonitorVolume,
                cfg.AudioSettings.SecondaryVolume);
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
            cfg.AudioSettings.SecondaryOutputDeviceId,
            cfg.AudioSettings.MonitorVolume,
            cfg.AudioSettings.SecondaryVolume);
    }

    private void ExportPhrases()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Export Phrases",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json",
            FileName = "tts-phrases"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            var json = _phraseService.ExportToJson();
            File.WriteAllText(dlg.FileName, json, System.Text.Encoding.UTF8);
            PhraseStatusMessage = $"Exported {Phrases.Count} phrase(s) to {Path.GetFileName(dlg.FileName)}.";
            _log.Info($"Phrases exported to '{dlg.FileName}'.");
        }
        catch (Exception ex)
        {
            PhraseStatusMessage = "Export failed — see log.";
            _log.Error("Phrase export failed", ex);
        }
    }

    private void ImportPhrases()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Import Phrases",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            var json = File.ReadAllText(dlg.FileName, System.Text.Encoding.UTF8);
            var result = _phraseService.ImportFromJson(json);
            if (!result.Success)
            {
                PhraseStatusMessage = $"Import failed: {result.ErrorMessage}";
                _log.Warn($"Phrase import failed: {result.ErrorMessage}");
                return;
            }

            LastImportResult = result;
            Refresh();
            ImportCompleted?.Invoke(this, EventArgs.Empty);
            _log.Info($"Imported {result.AddedCount} phrases from '{dlg.FileName}'.");
        }
        catch (Exception ex)
        {
            PhraseStatusMessage = "Import failed — see log.";
            _log.Error("Phrase import failed", ex);
        }
    }

    /// <summary>
    /// Caches audio for all phrases in the last import result.
    /// Call from the import progress dialog.
    /// </summary>
    public async Task CacheImportedPhrasesAsync(
        IProgress<(int current, int total)> progress,
        CancellationToken ct = default)
    {
        if (LastImportResult is null) return;
        var ids = LastImportResult.AddedPhraseIds;
        for (int i = 0; i < ids.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var phrase = _phraseService.GetById(ids[i]);
            if (phrase is not null)
                await _phraseCache.GenerateCacheAsync(phrase);
            progress.Report((i + 1, ids.Count));
        }
    }
}
