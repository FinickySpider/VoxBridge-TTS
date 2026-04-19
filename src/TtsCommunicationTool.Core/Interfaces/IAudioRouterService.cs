using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface IAudioRouterService : IDisposable
{
    Task PlayAsync(PlaybackRequest request, string? monitorDeviceId, string? secondaryDeviceId, CancellationToken ct = default);
    void StopAll();
    bool IsPlaying { get; }
    event EventHandler? PlaybackFinished;
}
