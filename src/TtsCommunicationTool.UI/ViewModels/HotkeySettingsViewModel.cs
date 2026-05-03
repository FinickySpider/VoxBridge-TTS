using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.Validation;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class HotkeySettingsViewModel : ViewModelBase
{
    private string _overlayHotkeyDisplay = string.Empty;
    private string _stopHotkeyDisplay = string.Empty;
    private string _settingsHotkeyDisplay = string.Empty;
    private string _resendHotkeyDisplay = string.Empty;
    private string _overrideHotkeyDisplay = string.Empty;
    private string _validationMessage = string.Empty;

    private HotkeyBinding _overlayHotkey = new();
    private HotkeyBinding _stopHotkey = new();
    private HotkeyBinding _settingsHotkey = new();
    private HotkeyBinding _resendHotkey = new();
    private HotkeyBinding _overrideHotkey = new();

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

    public string ResendHotkeyDisplay
    {
        get => _resendHotkeyDisplay;
        set => SetField(ref _resendHotkeyDisplay, value);
    }

    public string OverrideHotkeyDisplay
    {
        get => _overrideHotkeyDisplay;
        set => SetField(ref _overrideHotkeyDisplay, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetField(ref _validationMessage, value);
    }

    public HotkeyBinding OverlayHotkey => _overlayHotkey;
    public HotkeyBinding StopHotkey => _stopHotkey;
    public HotkeyBinding SettingsHotkey => _settingsHotkey;
    public HotkeyBinding ResendHotkey => _resendHotkey;
    public HotkeyBinding OverrideHotkey => _overrideHotkey;

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

    public void SetResendHotkey(HotkeyBinding binding)
    {
        if (!binding.IsEmpty)
        {
            var (valid, error) = HotkeyValidation.Validate(binding);
            if (!valid) { ValidationMessage = error!; return; }
            if (HotkeyValidation.AreConflicting(binding, _overlayHotkey))
            { ValidationMessage = "Resend hotkey conflicts with Overlay hotkey."; return; }
            if (HotkeyValidation.AreConflicting(binding, _stopHotkey))
            { ValidationMessage = "Resend hotkey conflicts with Stop hotkey."; return; }
            if (!_settingsHotkey.IsEmpty && HotkeyValidation.AreConflicting(binding, _settingsHotkey))
            { ValidationMessage = "Resend hotkey conflicts with Settings hotkey."; return; }
        }

        _resendHotkey = binding;
        ResendHotkeyDisplay = binding.IsEmpty ? "(none)" : binding.ToString();
        ValidationMessage = string.Empty;
    }

    public void ClearResendHotkey()
    {
        _resendHotkey = new HotkeyBinding();
        ResendHotkeyDisplay = "(none)";
        ValidationMessage = string.Empty;
    }

    /// <summary>
    /// Sets the overlay-internal override hotkey (stop + send). This is NOT a global Win32 hotkey.
    /// Accepts the standard validation rules but no conflict check against global hotkeys.
    /// Allow clearing (empty binding means the feature is unbound).
    /// </summary>
    public void SetOverrideHotkey(HotkeyBinding binding)
    {
        if (!binding.IsEmpty)
        {
            var (valid, error) = HotkeyValidation.Validate(binding);
            if (!valid) { ValidationMessage = error!; return; }
        }
        _overrideHotkey = binding;
        OverrideHotkeyDisplay = binding.IsEmpty ? "(none)" : binding.ToString();
        ValidationMessage = string.Empty;
    }

    public void ClearOverrideHotkey()
    {
        _overrideHotkey = new HotkeyBinding();
        OverrideHotkeyDisplay = "(none)";
        ValidationMessage = string.Empty;
    }

    public void LoadFrom(HotkeySettings s)
    {
        _overlayHotkey = s.OverlayHotkey;
        _stopHotkey = s.StopHotkey;
        _settingsHotkey = s.SettingsHotkey;
        _resendHotkey = s.ResendHotkey;
        _overrideHotkey = s.OverrideHotkey;
        OverlayHotkeyDisplay = s.OverlayHotkey.ToString();
        StopHotkeyDisplay = s.StopHotkey.ToString();
        SettingsHotkeyDisplay = s.SettingsHotkey.IsEmpty ? "(none)" : s.SettingsHotkey.ToString();
        ResendHotkeyDisplay = s.ResendHotkey.IsEmpty ? "(none)" : s.ResendHotkey.ToString();
        OverrideHotkeyDisplay = s.OverrideHotkey.IsEmpty ? "(none)" : s.OverrideHotkey.ToString();
        ValidationMessage = string.Empty;
    }

    public void ApplyTo(HotkeySettings s)
    {
        s.OverlayHotkey = _overlayHotkey;
        s.StopHotkey = _stopHotkey;
        s.SettingsHotkey = _settingsHotkey;
        s.ResendHotkey = _resendHotkey;
        s.OverrideHotkey = _overrideHotkey;
    }
}
