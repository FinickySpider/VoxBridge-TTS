using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface ITtsService : IAsyncDisposable
{
    Task InitializeAsync(CancellationToken ct = default);
    Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default);
    IReadOnlyList<VoiceInfo> GetAvailableVoices();
    bool IsInitialized { get; }
}
