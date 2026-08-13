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
    private readonly Sapi5TtsService _sapi5;
    private readonly IConfigService _config;

    public TtsRouter(KokoroTtsService kokoro, ElevenLabsTtsService elevenLabs, Sapi5TtsService sapi5, IConfigService config)
    {
        _kokoro = kokoro;
        _elevenLabs = elevenLabs;
        _sapi5 = sapi5;
        _config = config;
    }

    private ITtsService Active =>
        _config.CurrentConfig.VoiceSettings.Engine switch
        {
            VoiceEngine.ElevenLabs => _elevenLabs,
            VoiceEngine.Sapi5 => _sapi5,
            _ => _kokoro
        };

    /// <summary>Selects the service to use, respecting a per-request engine override.</summary>
    private ITtsService Resolve(TtsRequest request) =>
        request.EngineOverride switch
        {
            VoiceEngine.ElevenLabs => _elevenLabs,
            VoiceEngine.Kokoro     => _kokoro,
            VoiceEngine.Sapi5      => _sapi5,
            _                      => Active
        };

    public bool IsInitialized => Active.IsInitialized;

    /// <summary>Initialises local providers. ElevenLabs needs no local init.</summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _kokoro.InitializeAsync(ct).ConfigureAwait(false);
        await _sapi5.InitializeAsync(ct).ConfigureAwait(false);
    }

    public Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
        => Resolve(request).SynthesizeAsync(request, ct);

    public IReadOnlyList<VoiceInfo> GetAvailableVoices() => Active.GetAvailableVoices();

    public async ValueTask DisposeAsync()
    {
        await _kokoro.DisposeAsync().ConfigureAwait(false);
        await _elevenLabs.DisposeAsync().ConfigureAwait(false);
        await _sapi5.DisposeAsync().ConfigureAwait(false);
    }
}
