using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Tts;

/// <summary>
/// Routes TTS synthesis to either Kokoro or ElevenLabs based on the
/// currently configured VoiceEngine. Registered as the singleton ITtsService.
/// </summary>
public sealed class TtsRouter : ITtsService
{
    private readonly KokoroTtsService _kokoro;
    private readonly ElevenLabsTtsService _elevenLabs;
    private readonly IConfigService _config;

    public TtsRouter(KokoroTtsService kokoro, ElevenLabsTtsService elevenLabs, IConfigService config)
    {
        _kokoro = kokoro;
        _elevenLabs = elevenLabs;
        _config = config;
    }

    private ITtsService Active =>
        _config.CurrentConfig.VoiceSettings.Engine == VoiceEngine.ElevenLabs
            ? _elevenLabs
            : _kokoro;

    public bool IsInitialized => Active.IsInitialized;

    /// <summary>Always initialises Kokoro (offline model). ElevenLabs needs no local init.</summary>
    public Task InitializeAsync(CancellationToken ct = default) => _kokoro.InitializeAsync(ct);

    public Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
        => Active.SynthesizeAsync(request, ct);

    public IReadOnlyList<VoiceInfo> GetAvailableVoices() => Active.GetAvailableVoices();

    public async ValueTask DisposeAsync()
    {
        await _kokoro.DisposeAsync().ConfigureAwait(false);
        await _elevenLabs.DisposeAsync().ConfigureAwait(false);
    }
}
