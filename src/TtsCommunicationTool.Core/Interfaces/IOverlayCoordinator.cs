namespace TtsCommunicationTool.Core.Interfaces;

public interface IOverlayCoordinator
{
    bool IsOverlayVisible { get; }
    void ShowOverlay();
    void ShowOverlayWithText(string text);
    void HideOverlay();
    void ToggleOverlay();

    /// <summary>Raised when the user clicks the gear button in the overlay to open settings.</summary>
    event EventHandler? SettingsRequested;

    /// <summary>
    /// Immediately updates the overlay window's background opacity without saving.
    /// No-op if the overlay is not currently open.
    /// </summary>
    void SetLiveOpacity(double opacity);

    /// <summary>
    /// Immediately updates the overlay window's font family without saving.
    /// No-op if the overlay is not currently open.
    /// </summary>
    void SetLiveFont(string fontFamily);
}
