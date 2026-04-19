using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Tts;

/// <summary>
/// Stub TTS service. Will be replaced with Kokoro integration in SPRINT-02.
/// </summary>
public sealed class StubTtsService : ITtsService
{
    public bool IsInitialized { get; private set; }

    public Task InitializeAsync(CancellationToken ct = default)
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }

    public Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        // Generate a short silent WAV for testing purposes
        var sampleRate = 22050;
        var durationSeconds = 0.5;
        var sampleCount = (int)(sampleRate * durationSeconds);
        var audioData = new byte[sampleCount * 2]; // 16-bit silence
        return Task.FromResult(TtsResult.Ok(audioData, sampleRate));
    }

    public IReadOnlyList<VoiceInfo> GetAvailableVoices()
    {
        return new List<VoiceInfo>
        {
            new() { Id = "af_heart", DisplayName = "Heart (Stub)", EngineName = "Stub" }
        }.AsReadOnly();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
