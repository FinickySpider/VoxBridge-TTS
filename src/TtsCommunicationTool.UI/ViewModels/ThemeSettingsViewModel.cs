using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

/// <summary>
/// Backing ViewModel for the Theme settings tab.
/// Changes are applied live to the app via <see cref="IThemeService.Apply"/> as the user
/// edits; the parent <see cref="SettingsViewModel"/> IsDirty flag is driven by
/// <see cref="INotifyPropertyChanged"/> events bubbling up from this VM.
/// </summary>
public sealed class ThemeSettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly ILoggingService _log;
    private readonly IColorPickerService _colorPicker;

    // Working copy — mutated live; _savedSnapshot is what was last committed to disk.
    private ThemeSettings _workingCopy = ThemeDefaults.CreateDefault();
    private ThemeSettings _savedSnapshot = ThemeDefaults.CreateDefault();
    private bool _isDirty;
    private ThemeSettings? _selectedPreset;

    // ── Dirty / state ────────────────────────────────────────────────────────
    public bool IsDirty
    {
        get => _isDirty;
        private set => SetField(ref _isDirty, value);
    }

    /// <summary>True when the currently selected (saved) theme is a built-in seed.</summary>
    public bool IsEditingBuiltIn => _savedSnapshot.IsBuiltIn;

    // ── Theme name (always editable — becomes the name when saving) ──────────
    public string ThemeName
    {
        get => _workingCopy.Name;
        set
        {
            if (_workingCopy.Name == value) return;
            _workingCopy.Name = value;
            OnPropertyChanged();
            MarkDirty();
        }
    }

    // ── Preset list ──────────────────────────────────────────────────────────
    public ObservableCollection<ThemeSettings> AvailableThemes { get; } = new();

    public ThemeSettings? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (value is null || ReferenceEquals(_selectedPreset, value)) return;
            SetField(ref _selectedPreset, value);
            ApplyPreset(value);
        }
    }

    // ── Core colour properties ────────────────────────────────────────────────
    public string WindowBackground   { get => _workingCopy.WindowBackground;   set => SetColor(v => _workingCopy.WindowBackground   = v, value); }
    public string PanelBackground    { get => _workingCopy.PanelBackground;    set => SetColor(v => _workingCopy.PanelBackground    = v, value); }
    public string DeepPanelBackground{ get => _workingCopy.DeepPanelBackground;set => SetColor(v => _workingCopy.DeepPanelBackground= v, value); }
    public string HistoryBackground  { get => _workingCopy.HistoryBackground;  set => SetColor(v => _workingCopy.HistoryBackground  = v, value); }
    public string Surface0           { get => _workingCopy.Surface0;           set => SetColor(v => _workingCopy.Surface0           = v, value); }
    public string BorderColor        { get => _workingCopy.BorderColor;        set => SetColor(v => _workingCopy.BorderColor        = v, value); }
    public string Surface2           { get => _workingCopy.Surface2;           set => SetColor(v => _workingCopy.Surface2           = v, value); }
    public string Accent             { get => _workingCopy.Accent;             set => SetColor(v => _workingCopy.Accent             = v, value); }
    public string AccentHover        { get => _workingCopy.AccentHover;        set => SetColor(v => _workingCopy.AccentHover        = v, value); }

    // ── Text colour properties ────────────────────────────────────────────────
    public string PrimaryText   { get => _workingCopy.PrimaryText;   set => SetColor(v => _workingCopy.PrimaryText   = v, value); }
    public string SecondaryText { get => _workingCopy.SecondaryText; set => SetColor(v => _workingCopy.SecondaryText = v, value); }
    public string MutedText     { get => _workingCopy.MutedText;     set => SetColor(v => _workingCopy.MutedText     = v, value); }
    public string MutedIcon     { get => _workingCopy.MutedIcon;     set => SetColor(v => _workingCopy.MutedIcon     = v, value); }

    // ── Status colour properties ──────────────────────────────────────────────
    public string InfoColor   { get => _workingCopy.InfoColor;   set => SetColor(v => _workingCopy.InfoColor   = v, value); }
    public string ErrorColor  { get => _workingCopy.ErrorColor;  set => SetColor(v => _workingCopy.ErrorColor  = v, value); }
    public string ErrorHover  { get => _workingCopy.ErrorHover;  set => SetColor(v => _workingCopy.ErrorHover  = v, value); }
    public string Warning     { get => _workingCopy.Warning;     set => SetColor(v => _workingCopy.Warning     = v, value); }
    public string Success     { get => _workingCopy.Success;     set => SetColor(v => _workingCopy.Success     = v, value); }

    // ── Commands ─────────────────────────────────────────────────────────────
    public ICommand SaveCommand           { get; }
    public ICommand SaveAsCommand         { get; }
    public ICommand DuplicateCommand      { get; }
    public ICommand ResetEditsCommand     { get; }
    public ICommand DeleteCommand         { get; }
    /// <summary>Parameter: property name string (e.g. "Accent").</summary>
    public ICommand PickColorCommand      { get; }
    /// <summary>Parameter: property name string — resets that single colour to its default value.</summary>
    public ICommand ResetColorCommand     { get; }

    // ── Constructor ──────────────────────────────────────────────────────────
    public ThemeSettingsViewModel(IThemeService themeService, ILoggingService log, IColorPickerService colorPicker)
    {
        _themeService  = themeService;
        _log           = log;
        _colorPicker   = colorPicker;

        SaveCommand       = new AsyncRelayCommand(SaveAsync,       () => IsDirty);
        SaveAsCommand     = new AsyncRelayCommand(SaveAsPromptAsync);
        DuplicateCommand  = new AsyncRelayCommand(DuplicateAsync);
        ResetEditsCommand = new RelayCommand(RevertToSaved,        () => IsDirty);
        DeleteCommand     = new AsyncRelayCommand(DeleteAsync,     () => !IsEditingBuiltIn);
        PickColorCommand  = new RelayCommand(obj => PickColor(obj as string));
        ResetColorCommand = new RelayCommand(obj => ResetColorToDefault(obj as string));
    }

    // ── Load / revert ────────────────────────────────────────────────────────

    /// <summary>Called by SettingsViewModel when the settings window opens.</summary>
    public void LoadThemes(string activeThemeName)
    {
        AvailableThemes.Clear();
        foreach (var t in _themeService.LoadAll())
            AvailableThemes.Add(t);

        var active = AvailableThemes.FirstOrDefault(t =>
            string.Equals(t.Name, activeThemeName, StringComparison.OrdinalIgnoreCase))
            ?? AvailableThemes.First();

        _savedSnapshot = active.Clone();
        _savedSnapshot.IsBuiltIn = active.IsBuiltIn;
        _workingCopy   = active.Clone();
        _selectedPreset = active;
        OnPropertyChanged(nameof(SelectedPreset));
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        IsDirty = false;
    }

    /// <summary>
    /// Called by SettingsViewModel.CancelAsync() — reverts the live app colours to the
    /// last-saved theme state without persisting anything.
    /// </summary>
    public void RevertChanges()
    {
        _workingCopy = _savedSnapshot.Clone();
        _themeService.Apply(_workingCopy);
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        IsDirty = false;
    }

    /// <summary>
    /// Called by SettingsViewModel.SaveAsync() — persists the active theme choice and name.
    /// If editing a built-in, always saves as a NEW user theme.
    /// </summary>
    public async Task CommitAsync(IConfigService config)
    {
        if (!IsDirty) return;

        if (IsEditingBuiltIn)
        {
            // Save as a new user theme; name may have been edited in ThemeName TextBox
            await _themeService.SaveAsUserThemeAsync(_workingCopy, _workingCopy.Name);
            _log.Info($"Built-in theme forked as new user theme '{_workingCopy.Name}'.");
        }
        else
        {
            await _themeService.SaveAsUserThemeAsync(_workingCopy, _workingCopy.Name);
            _log.Info($"User theme '{_workingCopy.Name}' saved.");
        }

        // Persist active theme name to config
        config.CurrentConfig.ActiveThemeName = _workingCopy.Name;

        // Update snapshot and preset list
        _savedSnapshot = _workingCopy.Clone();
        _savedSnapshot.IsBuiltIn = false;
        RefreshPresetList();
        IsDirty = false;
        OnPropertyChanged(nameof(IsEditingBuiltIn));
    }

    // ── Private command implementations ──────────────────────────────────────

    private async Task SaveAsync()
    {
        if (IsEditingBuiltIn)
        {
            // Saving a built-in always creates a fork — ensure user knows by the banner in UI.
            await _themeService.SaveAsUserThemeAsync(_workingCopy, _workingCopy.Name);
            _savedSnapshot = _workingCopy.Clone();
            _savedSnapshot.IsBuiltIn = false;
            RefreshPresetList();
            IsDirty = false;
            OnPropertyChanged(nameof(IsEditingBuiltIn));
            _log.Info($"New theme '{_workingCopy.Name}' created from built-in.");
        }
        else
        {
            await _themeService.SaveAsUserThemeAsync(_workingCopy, _workingCopy.Name);
            _savedSnapshot = _workingCopy.Clone();
            IsDirty = false;
            _log.Info($"Theme '{_workingCopy.Name}' saved.");
        }
    }

    private async Task SaveAsPromptAsync()
    {
        var name = PromptName("Save Theme As", _workingCopy.Name + " Copy");
        if (name is null) return;
        _workingCopy.Name = name;
        OnPropertyChanged(nameof(ThemeName));
        await _themeService.SaveAsUserThemeAsync(_workingCopy, name);
        _savedSnapshot = _workingCopy.Clone();
        _savedSnapshot.IsBuiltIn = false;
        RefreshPresetList();
        IsDirty = false;
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        _log.Info($"Theme saved as '{name}'.");
    }

    private async Task DuplicateAsync()
    {
        var name = _workingCopy.Name + " Copy";
        await _themeService.SaveAsUserThemeAsync(_workingCopy, name);
        RefreshPresetList();
        _log.Info($"Theme duplicated as '{name}'.");
    }

    private void RevertToSaved()
    {
        _workingCopy = _savedSnapshot.Clone();
        _themeService.Apply(_workingCopy);
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        IsDirty = false;
    }

    private async Task DeleteAsync()
    {
        if (IsEditingBuiltIn) return;
        await _themeService.DeleteUserThemeAsync(_savedSnapshot.Name);
        // Fall back to Default Dark
        LoadThemes("Default Dark");
        _themeService.Apply(_workingCopy);
        _log.Info($"Theme '{_savedSnapshot.Name}' deleted. Reverted to Default Dark.");
    }

    private void ApplyPreset(ThemeSettings preset)
    {
        _savedSnapshot = preset.Clone();
        _savedSnapshot.IsBuiltIn = preset.IsBuiltIn;
        _workingCopy = preset.Clone();
        _themeService.Apply(_workingCopy);
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        IsDirty = false;
    }

    private void PickColor(string? propertyName)
    {
        if (propertyName is null) return;
        var currentHex = GetColorProperty(propertyName);
        if (currentHex is null) return;

        var hex = _colorPicker.PickColor(currentHex);
        if (hex is null) return;
        SetColorByName(propertyName, hex);
    }

    private void ResetColorToDefault(string? propertyName)
    {
        if (propertyName is null) return;
        var defaults = ThemeDefaults.CreateDefault();
        var defaultHex = GetColorPropertyFrom(defaults, propertyName);
        if (defaultHex is null) return;
        SetColorByName(propertyName, defaultHex);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetColor(Action<string> setter, string value, [CallerMemberName] string? propName = null)
    {
        if (!IsValidHex(value)) return;
        setter(value);
        _themeService.Apply(_workingCopy);
        if (propName is not null) OnPropertyChanged(propName);
        MarkDirty();
    }

    private void MarkDirty()
    {
        IsDirty = true;
    }

    private void RaiseAllColourProperties()
    {
        foreach (var name in ColourPropertyNames)
            OnPropertyChanged(name);
    }

    private static readonly string[] ColourPropertyNames =
    {
        nameof(WindowBackground), nameof(PanelBackground), nameof(DeepPanelBackground),
        nameof(HistoryBackground), nameof(Surface0), nameof(BorderColor), nameof(Surface2),
        nameof(Accent), nameof(AccentHover),
        nameof(PrimaryText), nameof(SecondaryText), nameof(MutedText), nameof(MutedIcon),
        nameof(InfoColor), nameof(ErrorColor), nameof(ErrorHover), nameof(Warning), nameof(Success),
    };

    private string? GetColorProperty(string propertyName) =>
        GetColorPropertyFrom(_workingCopy, propertyName);

    private static string? GetColorPropertyFrom(ThemeSettings t, string name) => name switch
    {
        nameof(WindowBackground)    => t.WindowBackground,
        nameof(PanelBackground)     => t.PanelBackground,
        nameof(DeepPanelBackground) => t.DeepPanelBackground,
        nameof(HistoryBackground)   => t.HistoryBackground,
        nameof(Surface0)            => t.Surface0,
        nameof(BorderColor)         => t.BorderColor,
        nameof(Surface2)            => t.Surface2,
        nameof(Accent)              => t.Accent,
        nameof(AccentHover)         => t.AccentHover,
        nameof(PrimaryText)         => t.PrimaryText,
        nameof(SecondaryText)       => t.SecondaryText,
        nameof(MutedText)           => t.MutedText,
        nameof(MutedIcon)           => t.MutedIcon,
        nameof(InfoColor)           => t.InfoColor,
        nameof(ErrorColor)          => t.ErrorColor,
        nameof(ErrorHover)          => t.ErrorHover,
        nameof(Warning)             => t.Warning,
        nameof(Success)             => t.Success,
        _ => null
    };

    private void SetColorByName(string name, string hex)
    {
        switch (name)
        {
            case nameof(WindowBackground):    WindowBackground    = hex; break;
            case nameof(PanelBackground):     PanelBackground     = hex; break;
            case nameof(DeepPanelBackground): DeepPanelBackground = hex; break;
            case nameof(HistoryBackground):   HistoryBackground   = hex; break;
            case nameof(Surface0):            Surface0            = hex; break;
            case nameof(BorderColor):         BorderColor         = hex; break;
            case nameof(Surface2):            Surface2            = hex; break;
            case nameof(Accent):              Accent              = hex; break;
            case nameof(AccentHover):         AccentHover         = hex; break;
            case nameof(PrimaryText):         PrimaryText         = hex; break;
            case nameof(SecondaryText):       SecondaryText       = hex; break;
            case nameof(MutedText):           MutedText           = hex; break;
            case nameof(MutedIcon):           MutedIcon           = hex; break;
            case nameof(InfoColor):           InfoColor           = hex; break;
            case nameof(ErrorColor):          ErrorColor          = hex; break;
            case nameof(ErrorHover):          ErrorHover          = hex; break;
            case nameof(Warning):             Warning             = hex; break;
            case nameof(Success):             Success             = hex; break;
        }
    }

    private void RefreshPresetList()
    {
        var current = SelectedPreset?.Name;
        AvailableThemes.Clear();
        foreach (var t in _themeService.LoadAll())
            AvailableThemes.Add(t);
        _selectedPreset = AvailableThemes.FirstOrDefault(t => t.Name == _workingCopy.Name)
                          ?? AvailableThemes.First();
        OnPropertyChanged(nameof(SelectedPreset));
    }

    private static bool IsValidHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return false;
        try { ColorConverter.ConvertFromString(hex); return true; }
        catch { return false; }
    }

    // ── Name prompt (minimal WPF input window) ────────────────────────────────
    private static string? PromptName(string title, string defaultValue)
    {
        string? result = null;
        var bg = Application.Current.Resources["WindowBackgroundBrush"] as SolidColorBrush
                 ?? new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
        var fg = Application.Current.Resources["PrimaryTextBrush"] as SolidColorBrush
                 ?? new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4));
        var border = Application.Current.Resources["BorderBrush"] as SolidColorBrush
                     ?? new SolidColorBrush(Color.FromRgb(0x45, 0x47, 0x5A));

        var tb = new System.Windows.Controls.TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(12, 12, 12, 8),
            Padding = new Thickness(8, 6, 8, 6),
            Background = Application.Current.Resources["PanelBackgroundBrush"] as SolidColorBrush ?? bg,
            Foreground = fg,
            BorderBrush = border,
            BorderThickness = new Thickness(1),
            FontSize = 13,
            CaretBrush = Application.Current.Resources["AccentBrush"] as SolidColorBrush ?? fg,
        };

        var ok = new System.Windows.Controls.Button
        {
            Content = "OK",
            Width = 80, Height = 30,
            Margin = new Thickness(0, 0, 8, 0),
            Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush ?? fg,
            Foreground = bg,
            BorderThickness = new Thickness(0),
        };
        var cancel = new System.Windows.Controls.Button
        {
            Content = "Cancel",
            Width = 80, Height = 30,
            Background = Application.Current.Resources["Surface2Brush"] as SolidColorBrush ?? border,
            Foreground = fg,
            BorderThickness = new Thickness(0),
        };

        var btns = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12, 0, 12, 12),
        };
        btns.Children.Add(ok);
        btns.Children.Add(cancel);

        var panel = new System.Windows.Controls.StackPanel();
        panel.Children.Add(tb);
        panel.Children.Add(btns);

        var win = new Window
        {
            Title = title,
            Width = 360, Height = 130,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Background = bg,
            Content = panel,
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
        };

        ok.Click     += (_, _) => { result = tb.Text.Trim(); win.Close(); };
        cancel.Click += (_, _) => win.Close();
        win.Loaded   += (_, _) => { tb.SelectAll(); tb.Focus(); };
        tb.KeyDown   += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)  { result = tb.Text.Trim(); win.Close(); }
            if (e.Key == System.Windows.Input.Key.Escape) { win.Close(); }
        };

        win.ShowDialog();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}
