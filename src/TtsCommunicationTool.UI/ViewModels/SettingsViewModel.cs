using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private int _selectedTabIndex;

    public GeneralSettingsViewModel General { get; }
    public HotkeySettingsViewModel Hotkeys { get; }
    public AudioSettingsViewModel Audio { get; }
    public VoiceSettingsViewModel Voice { get; }
    public AppearanceSettingsViewModel Appearance { get; }
    public PhraseListViewModel Phrases { get; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetField(ref _selectedTabIndex, value);
    }

    public bool IsFirstRun { get; set; }

    public ICommand SaveCommand { get; }
    public ICommand ResetDefaultsCommand { get; }

    public event EventHandler? Saved;

    public SettingsViewModel(
        IConfigService config,
        ILoggingService log,
        GeneralSettingsViewModel general,
        HotkeySettingsViewModel hotkeys,
        AudioSettingsViewModel audio,
        VoiceSettingsViewModel voice,
        AppearanceSettingsViewModel appearance,
        PhraseListViewModel phrases)
    {
        _config = config;
        _log = log;
        General = general;
        Hotkeys = hotkeys;
        Audio = audio;
        Voice = voice;
        Appearance = appearance;
        Phrases = phrases;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ResetDefaultsCommand = new RelayCommand(ResetDefaults);

        LoadFromConfig();
    }

    private void LoadFromConfig()
    {
        var cfg = _config.CurrentConfig;
        General.LoadFrom(cfg.GeneralSettings);
        Hotkeys.LoadFrom(cfg.HotkeySettings);
        Audio.LoadFrom(cfg.AudioSettings);
        Voice.LoadFrom(cfg.VoiceSettings);
        Appearance.LoadFrom(cfg.OverlaySettings);
    }

    private async Task SaveAsync()
    {
        var cfg = _config.CurrentConfig;
        General.ApplyTo(cfg.GeneralSettings);
        Hotkeys.ApplyTo(cfg.HotkeySettings);
        Audio.ApplyTo(cfg.AudioSettings);
        Voice.ApplyTo(cfg.VoiceSettings);
        Appearance.ApplyTo(cfg.OverlaySettings);

        await _config.SaveAsync(cfg);
        _log.Info("Settings saved.");
        Saved?.Invoke(this, EventArgs.Empty);
    }

    private void ResetDefaults()
    {
        var defaults = _config.GetDefaults();
        General.LoadFrom(defaults.GeneralSettings);
        Hotkeys.LoadFrom(defaults.HotkeySettings);
        Audio.LoadFrom(defaults.AudioSettings);
        Voice.LoadFrom(defaults.VoiceSettings);
        Appearance.LoadFrom(defaults.OverlaySettings);
        _log.Info("Settings reset to defaults.");
    }
}
