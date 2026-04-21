using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.Validation;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class HotkeySettingsViewModel : ViewModelBase
{
    private string _overlayHotkeyDisplay = string.Empty;
    private string _stopHotkeyDisplay = string.Empty;
    private string _settingsHotkeyDisplay = string.Empty;
    private string _validationMessage = string.Empty;

    private HotkeyBinding _overlayHotkey = new();
    private HotkeyBinding _stopHotkey = new();
    private HotkeyBinding _settingsHotkey = new();

    public string OverlayHotkeyDisplay
    {
        get => _overlayHotkeyDisplay;
        set => SetField(ref _overlayHotkeyDisplay, value);
    }

    public string StopHotkeyDisplay
    {
        get => _stopHotkeyDisplay;
        set => SetField(ref _stopHotkeyDisplay, value);
    }

    public string SettingsHotkeyDisplay
    {
        get => _settingsHotkeyDisplay;
        set => SetField(ref _settingsHotkeyDisplay, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetField(ref _validationMessage, value);
    }

    public HotkeyBinding OverlayHotkey => _overlayHotkey;
    public HotkeyBinding StopHotkey => _stopHotkey;
    public HotkeyBinding SettingsHotkey => _settingsHotkey;

    public void ClearOverlayHotkey()
    {
        _overlayHotkey = new HotkeyBinding();
        OverlayHotkeyDisplay = "(none)";
        ValidationMessage = string.Empty;
    }

    public void ClearStopHotkey()
    {
        _stopHotkey = new HotkeyBinding();
        StopHotkeyDisplay = "(none)";
        ValidationMessage = string.Empty;
    }

    public void ClearSettingsHotkey()
    {
        _settingsHotkey = new HotkeyBinding();
        SettingsHotkeyDisplay = "(none)";
        ValidationMessage = string.Empty;
    }

    public void SetOverlayHotkey(HotkeyBinding binding)
    {
        var (valid, error) = HotkeyValidation.Validate(binding);
        if (!valid)
        {
            ValidationMessage = error!;
            return;
        }

        if (HotkeyValidation.AreConflicting(binding, _stopHotkey))
        {
            ValidationMessage = "Overlay hotkey conflicts with Stop hotkey.";
            return;
        }

        if (!_settingsHotkey.IsEmpty && HotkeyValidation.AreConflicting(binding, _settingsHotkey))
        {
            ValidationMessage = "Overlay hotkey conflicts with Settings hotkey.";
            return;
        }

        _overlayHotkey = binding;
        OverlayHotkeyDisplay = binding.ToString();
        ValidationMessage = string.Empty;
    }

    public void SetStopHotkey(HotkeyBinding binding)
    {
        var (valid, error) = HotkeyValidation.Validate(binding);
        if (!valid)
        {
            ValidationMessage = error!;
            return;
        }

        if (HotkeyValidation.AreConflicting(binding, _overlayHotkey))
        {
            ValidationMessage = "Stop hotkey conflicts with Overlay hotkey.";
            return;
        }

        if (!_settingsHotkey.IsEmpty && HotkeyValidation.AreConflicting(binding, _settingsHotkey))
        {
            ValidationMessage = "Stop hotkey conflicts with Settings hotkey.";
            return;
        }

        _stopHotkey = binding;
        StopHotkeyDisplay = binding.ToString();
        ValidationMessage = string.Empty;
    }

    public void SetSettingsHotkey(HotkeyBinding binding)
    {
        // Allow clearing by pressing Escape (empty binding is acceptable)
        if (!binding.IsEmpty)
        {
            var (valid, error) = HotkeyValidation.Validate(binding);
            if (!valid)
            {
                ValidationMessage = error!;
                return;
            }

            if (HotkeyValidation.AreConflicting(binding, _overlayHotkey))
            {
                ValidationMessage = "Settings hotkey conflicts with Overlay hotkey.";
                return;
            }

            if (HotkeyValidation.AreConflicting(binding, _stopHotkey))
            {
                ValidationMessage = "Settings hotkey conflicts with Stop hotkey.";
                return;
            }
        }

        _settingsHotkey = binding;
        SettingsHotkeyDisplay = binding.IsEmpty ? "(none)" : binding.ToString();
        ValidationMessage = string.Empty;
    }

    public void LoadFrom(HotkeySettings s)
    {
        _overlayHotkey = s.OverlayHotkey;
        _stopHotkey = s.StopHotkey;
        _settingsHotkey = s.SettingsHotkey;
        OverlayHotkeyDisplay = s.OverlayHotkey.ToString();
        StopHotkeyDisplay = s.StopHotkey.ToString();
        SettingsHotkeyDisplay = s.SettingsHotkey.IsEmpty ? "(none)" : s.SettingsHotkey.ToString();
        ValidationMessage = string.Empty;
    }

    public void ApplyTo(HotkeySettings s)
    {
        s.OverlayHotkey = _overlayHotkey;
        s.StopHotkey = _stopHotkey;
        s.SettingsHotkey = _settingsHotkey;
    }
}
