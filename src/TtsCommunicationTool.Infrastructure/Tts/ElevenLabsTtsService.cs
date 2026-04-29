using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NAudio.Wave;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Tts;

/// <summary>
/// ITtsService implementation backed by the ElevenLabs REST API.
/// MP3 responses are decoded to 16-bit 44100 Hz mono PCM for playback.
/// </summary>
public sealed class ElevenLabsTtsService : ITtsService
{
    private const string BaseUrl = "https://api.elevenlabs.io/v1";

    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly HttpClient _http;

    private List<VoiceInfo> _cachedVoices = new();

    public ElevenLabsTtsService(IConfigService config, ILoggingService log, HttpClient http)
    {
        _config = config;
        _log = log;
        _http = http;
    }

    // ── ITtsService ──────────────────────────────────────────────────────────

    public bool IsInitialized => !string.IsNullOrWhiteSpace(_config.CurrentConfig.ElevenLabs.ApiKey);

    public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;

    public IReadOnlyList<VoiceInfo> GetAvailableVoices() => _cachedVoices;

    public async Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        var voiceId = string.IsNullOrWhiteSpace(request.VoiceId) ? settings.SelectedVoiceId : request.VoiceId;

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            return TtsResult.Fail("ElevenLabs API key is not configured.");
        if (string.IsNullOrWhiteSpace(voiceId))
            return TtsResult.Fail("No ElevenLabs voice selected.");

        try
        {
            var url = $"{BaseUrl}/text-to-speech/{voiceId}";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("xi-api-key", settings.ApiKey);
            req.Headers.Add("Accept", "audio/mpeg");

            var body = new { text = request.Text, model_id = settings.ModelId };
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _log.Error($"ElevenLabs API error {resp.StatusCode}: {err}");
                return TtsResult.Fail($"ElevenLabs API returned {(int)resp.StatusCode}: {resp.ReasonPhrase}");
            }

            var mp3Bytes = await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            var (pcm, sampleRate, channels, bps) = DecodeMp3ToPcm(mp3Bytes);
            return TtsResult.Ok(pcm, sampleRate, channels, bps);
        }
        catch (Exception ex)
        {
            _log.Error("ElevenLabs synthesis failed", ex);
            return TtsResult.Fail($"ElevenLabs error: {ex.Message}");
        }
    }

    // ── Public helpers ───────────────────────────────────────────────────────

    /// <summary>Fetches available voices from ElevenLabs and updates the cache.</summary>
    public async Task<List<VoiceInfo>> FetchVoicesAsync(CancellationToken ct = default)
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            return new List<VoiceInfo>();

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/voices");
            req.Headers.Add("xi-api-key", settings.ApiKey);

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            var voices = new List<VoiceInfo>();
            foreach (var v in doc.RootElement.GetProperty("voices").EnumerateArray())
            {
                voices.Add(new VoiceInfo
                {
                    Id = v.GetProperty("voice_id").GetString() ?? string.Empty,
                    DisplayName = v.GetProperty("name").GetString() ?? string.Empty,
                    EngineName = "ElevenLabs"
                });
            }

            _cachedVoices = voices;
            _log.Info($"ElevenLabs: fetched {voices.Count} voices.");
            return voices;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to fetch ElevenLabs voices", ex);
            return new List<VoiceInfo>();
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static (byte[] pcm, int sampleRate, int channels, int bitsPerSample) DecodeMp3ToPcm(byte[] mp3)
    {
        using var ms = new System.IO.MemoryStream(mp3);
        using var reader = new Mp3FileReader(ms);

        // Convert to 16-bit PCM at the source sample rate
        var targetFormat = new WaveFormat(reader.WaveFormat.SampleRate, 16, reader.WaveFormat.Channels);
        using var converter = new WaveFormatConversionStream(targetFormat, reader);
        using var pcmMs = new System.IO.MemoryStream();
        converter.CopyTo(pcmMs);
        return (pcmMs.ToArray(), targetFormat.SampleRate, targetFormat.Channels, 16);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
