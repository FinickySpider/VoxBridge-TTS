namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Service that hosts the global hotkey message pump.
/// </summary>
public interface IHotkeyHost
{
    /// <summary>
    /// Re-registers all phrase hotkeys from current config.
    /// Call after phrase hotkey changes to refresh.
    /// </summary>
    void RegisterPhraseHotkeys();

    /// <summary>
    /// Unregisters all hotkeys and re-registers everything from current config.
    /// Call after settings save to pick up changed overlay/stop/phrase hotkeys.
    /// </summary>
    void RefreshAllHotkeys();
}
