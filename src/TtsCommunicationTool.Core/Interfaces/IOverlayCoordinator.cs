namespace TtsCommunicationTool.Core.Interfaces;

public interface IOverlayCoordinator
{
    bool IsOverlayVisible { get; }
    void ShowOverlay();
    void HideOverlay();
    void ToggleOverlay();
}
