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
}
