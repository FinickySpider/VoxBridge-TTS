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
}
