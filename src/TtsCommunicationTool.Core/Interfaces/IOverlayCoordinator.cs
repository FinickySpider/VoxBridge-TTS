namespace TtsCommunicationTool.Core.Interfaces;

public interface IOverlayCoordinator
{
    bool IsOverlayVisible { get; }
    void ShowOverlay();
    void ShowOverlayWithText(string text);
    void HideOverlay();
    void ToggleOverlay();
}
