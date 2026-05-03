namespace TtsCommunicationTool.Core.Models;

public sealed class HotkeySettings
{
    public HotkeyBinding OverlayHotkey { get; set; } = new()
    {
        Ctrl = true,
        Shift = true,
        Key = "Space"
    };

    public HotkeyBinding StopHotkey { get; set; } = new()
    {
        Ctrl = true,
        Shift = true,
        Key = "Back"
    };

    /// <summary>Global hotkey to open the Settings window. Empty = unbound.</summary>
    public HotkeyBinding SettingsHotkey { get; set; } = new();

    /// <summary>Global hotkey to resend the last spoken message. Empty = unbound.</summary>
    public HotkeyBinding ResendHotkey { get; set; } = new();

    /// <summary>
    /// In-overlay hotkey that immediately stops any current audio and sends the
    /// typed overlay text. Only active while the overlay window is open.
    /// Default: Ctrl+Enter.
    /// </summary>
    public HotkeyBinding OverrideHotkey { get; set; } = new()
    {
        Ctrl = true,
        Key = "Return"
    };
}
