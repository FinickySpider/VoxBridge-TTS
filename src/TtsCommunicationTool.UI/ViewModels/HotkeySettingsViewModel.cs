using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.Validation;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class HotkeySettingsViewModel : ViewModelBase
{
    private string _overlayHotkeyDisplay = string.Empty;
    private string _stopHotkeyDisplay = string.Empty;
    private string _validationMessage = string.Empty;

    private HotkeyBinding _overlayHotkey = new();
    private HotkeyBinding _stopHotkey = new();

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

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetField(ref _validationMessage, value);
    }

    public HotkeyBinding OverlayHotkey => _overlayHotkey;
    public HotkeyBinding StopHotkey => _stopHotkey;

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

        _stopHotkey = binding;
        StopHotkeyDisplay = binding.ToString();
        ValidationMessage = string.Empty;
    }

    public void LoadFrom(HotkeySettings s)
    {
        _overlayHotkey = s.OverlayHotkey;
        _stopHotkey = s.StopHotkey;
        OverlayHotkeyDisplay = s.OverlayHotkey.ToString();
        StopHotkeyDisplay = s.StopHotkey.ToString();
        ValidationMessage = string.Empty;
    }

    public void ApplyTo(HotkeySettings s)
    {
        s.OverlayHotkey = _overlayHotkey;
        s.StopHotkey = _stopHotkey;
    }
}
