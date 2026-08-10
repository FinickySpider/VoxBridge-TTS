using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using NAudio.Wave;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Logging;
using TtsCommunicationTool.Infrastructure.Security;

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

    public bool IsInitialized => !string.IsNullOrEmpty(_config.CurrentConfig.ElevenLabs.EncryptedApiKey);

    public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;

    public IReadOnlyList<VoiceInfo> GetAvailableVoices() => _cachedVoices;

    public async Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        var voiceId = string.IsNullOrWhiteSpace(request.VoiceId) ? settings.SelectedVoiceId : request.VoiceId;

        var apiKey = ApiKeyVault.Decrypt(settings.EncryptedApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
            return TtsResult.Fail("ElevenLabs API key is not configured.");
        if (string.IsNullOrWhiteSpace(voiceId))
            return TtsResult.Fail("No ElevenLabs voice selected.");

        try
        {
            var sw = Stopwatch.StartNew();
            var url = $"{BaseUrl}/text-to-speech/{voiceId}";

            _log.LogEvent(DiagnosticLogLevel.Info, "api", "api_request_started",
                "ElevenLabs TTS API request started",
                new
                {
                    provider    = "elevenlabs",
                    voice_id    = voiceId,
                    text_length = request.Text.Length,
                    text_hash   = FileLoggingService.ComputeTextHash(request.Text),
                    text        = _log.LogRawText ? request.Text : (string?)null,
                    request_id  = _log.IncludeRequestIds ? request.RequestId : null
                });

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("xi-api-key", apiKey);
            req.Headers.Add("Accept", "audio/mpeg");

            var body = new { text = request.Text, model_id = settings.ModelId };
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _log.Error($"ElevenLabs API error {resp.StatusCode}: {err}");
                var statusCode = (int)resp.StatusCode;
                _log.LogEvent(statusCode == 429 ? DiagnosticLogLevel.Warn : DiagnosticLogLevel.Error,
                    "api",
                    statusCode == 429 ? "api_rate_limited" : "api_request_failed",
                    $"ElevenLabs API returned {statusCode}",
                    new { provider = "elevenlabs", status = statusCode, error = err, request_id = _log.IncludeRequestIds ? request.RequestId : null });

                // Parse ElevenLabs JSON error body for a human-readable detail message.
                var detail = TryParseElevenLabsError(err);
                var userMsg = detail ?? resp.ReasonPhrase ?? statusCode.ToString();
                // Include the voice ID in 404s so the user can see what was sent.
                if (statusCode == 404)
                    userMsg = $"{userMsg} (voice_id=\"{voiceId}\")";
                return TtsResult.Fail($"ElevenLabs API returned {statusCode}: {userMsg}");
            }

            var mp3Bytes = await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            sw.Stop();
            _log.LogEvent(DiagnosticLogLevel.Info, "api", "api_request_completed",
                "ElevenLabs TTS API request completed",
                new { provider = "elevenlabs", status = 200, duration_ms = (long)sw.Elapsed.TotalMilliseconds, request_id = _log.IncludeRequestIds ? request.RequestId : null });

            // Track cumulative character usage across sessions and fire-and-forget persist.
            _config.CurrentConfig.ElevenLabs.TotalCharactersUsed += request.Text.Length;
            _ = _config.SaveAsync(_config.CurrentConfig);

            var (pcm, sampleRate, channels, bps) = DecodeMp3ToPcm(mp3Bytes);
            var playbackRate = Math.Clamp(request.Pitch, 0.5f, 2.0f)
                              * Math.Clamp(request.Speed, 0.5f, 2.0f);
            var playbackSampleRate = (int)(sampleRate * playbackRate);
            return TtsResult.Ok(pcm, playbackSampleRate, channels, bps);
        }
        catch (Exception ex)
        {
            _log.Error("ElevenLabs synthesis failed", ex);
            _log.LogEvent(DiagnosticLogLevel.Error, "api", "api_request_failed",
                "ElevenLabs synthesis failed",
                new { provider = "elevenlabs", error = ex.Message, request_id = _log.IncludeRequestIds ? request.RequestId : null });
            return TtsResult.Fail($"ElevenLabs error: {ex.Message}");
        }
    }

    // ── API key vault helpers (called from ViewModel) ────────────────────────

    /// <summary>Returns true if an encrypted API key is present in config.</summary>
    public bool HasApiKey() => !string.IsNullOrEmpty(_config.CurrentConfig.ElevenLabs.EncryptedApiKey);

    /// <summary>
    /// Encrypts <paramref name="plaintext"/> with DPAPI and persists it to config.
    /// Also stores the 4-char tail and the current date for masked display.
    /// The legacy plain-text <c>ApiKey</c> field is cleared.
    /// </summary>
    public void SaveApiKey(string plaintext)
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        settings.EncryptedApiKey    = ApiKeyVault.Encrypt(plaintext);
        settings.ApiKeyTail         = ApiKeyVault.ComputeTail(plaintext);
        settings.ApiKeyUpdatedDate  = DateTime.Today.ToString("yyyy-MM-dd");
        settings.ApiKey             = string.Empty;  // clear legacy field
        _ = _config.SaveAsync(_config.CurrentConfig);
    }

    /// <summary>Clears all stored API key data from config and persists immediately.</summary>
    public void ClearApiKey()
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        settings.EncryptedApiKey   = null;
        settings.ApiKeyTail        = null;
        settings.ApiKeyUpdatedDate = null;
        settings.ApiKey            = string.Empty;
        _ = _config.SaveAsync(_config.CurrentConfig);
    }

    /// <summary>
    /// Returns a masked display string for the UI, e.g. "sk_...a1b2   Updated 2026-05-03".
    /// Returns an empty string if no key is stored.
    /// </summary>
    public string GetApiKeyMaskedDisplay()
    {
        var s = _config.CurrentConfig.ElevenLabs;
        if (string.IsNullOrEmpty(s.EncryptedApiKey)) return string.Empty;
        var tail = s.ApiKeyTail ?? "????";
        var date = s.ApiKeyUpdatedDate ?? "unknown";
        return $"sk_...{tail}   Updated {date}";
    }

    /// <summary>
    /// If the legacy plain <c>ApiKey</c> field is populated and <c>EncryptedApiKey</c> is null,
    /// encrypts and migrates it in-place, then returns <c>true</c>.
    /// </summary>
    public bool MigrateLegacyApiKey()
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        if (!string.IsNullOrEmpty(settings.ApiKey) && string.IsNullOrEmpty(settings.EncryptedApiKey))
        {
            SaveApiKey(settings.ApiKey);
            return true;
        }
        return false;
    }

    // ── Public helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Tests a specific API key (which may be an unsaved in-memory value) by hitting
    /// <c>/v1/user/subscription</c>.  Returns <c>(true, success-message)</c> or <c>(false, error-message)</c>.
    /// The key is never stored or logged.
    /// </summary>
    public async Task<(bool ok, string message)> TestApiKeyAsync(string plaintext, CancellationToken ct = default)    {
        if (string.IsNullOrWhiteSpace(plaintext))
            return (false, "No API key provided.");
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/user/subscription");
            req.Headers.Add("xi-api-key", plaintext);
            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            if (resp.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
                return (false, "Invalid or unauthorized key.");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var limit = doc.RootElement.GetProperty("character_limit").GetInt32();
            var used  = doc.RootElement.GetProperty("character_count").GetInt32();
            var tier  = doc.RootElement.TryGetProperty("tier", out var t) ? (t.GetString() ?? "unknown") : "unknown";
            var remaining = Math.Max(0, limit - used);
            return (true, $"API key is valid. Plan: {tier}, {remaining:N0} chars remaining.");
        }
        catch (Exception ex)
        {
            _log.Error("ElevenLabs API key test failed", ex);
            return (false, "Could not reach ElevenLabs. Check your connection.");
        }
        finally
        {
            // Ensure the caller's local variable holding plaintext goes out of scope ASAP.
            // We don't own the caller's stack frame, but we can at least null our own reference.
        }
    }

    /// <summary>
    /// Decrypts the currently-stored API key and tests it against <c>/v1/user/subscription</c>.
    /// Keeps the decrypted bytes inside this layer so they never reach the ViewModel.
    /// </summary>
    public async Task<(bool ok, string message)> TestSavedApiKeyAsync(CancellationToken ct = default)
    {
        var plaintext = ApiKeyVault.Decrypt(_config.CurrentConfig.ElevenLabs.EncryptedApiKey);
        if (string.IsNullOrWhiteSpace(plaintext))
            return (false, "No stored API key found.");
        return await TestApiKeyAsync(plaintext, ct).ConfigureAwait(false);
    }

    /// <summary>Fetches available voices from ElevenLabs and updates the cache.</summary>
    public async Task<List<VoiceInfo>> FetchVoicesAsync(CancellationToken ct = default)
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        var apiKey = ApiKeyVault.Decrypt(settings.EncryptedApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
            return new List<VoiceInfo>();

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/voices");
            req.Headers.Add("xi-api-key", apiKey);

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            var voices = new List<VoiceInfo>();
            foreach (var v in doc.RootElement.GetProperty("voices").EnumerateArray())
            {
                var category = v.TryGetProperty("category", out var cat) ? cat.GetString() : null;
                voices.Add(new VoiceInfo
                {
                    Id = v.GetProperty("voice_id").GetString() ?? string.Empty,
                    DisplayName = v.GetProperty("name").GetString() ?? string.Empty,
                    EngineName = "ElevenLabs",
                    // Cloned and AI-generated voices require Creator or higher plan.
                    IsCustomVoice = category is "cloned" or "generated"
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

    /// <summary>
    /// Fetches the user's character usage and quota for the current billing period.
    /// Stores the result in config (.SubscriptionCharacterCount / .SubscriptionCharacterLimit).
    /// Returns (used, limit); returns (0, 0) on failure.
    /// </summary>
    public async Task<(int used, int limit)> FetchUserSubscriptionAsync(CancellationToken ct = default)
    {
        var settings = _config.CurrentConfig.ElevenLabs;
        var apiKey = ApiKeyVault.Decrypt(settings.EncryptedApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
            return (0, 0);

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/user/subscription");
            req.Headers.Add("xi-api-key", apiKey);

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            var used  = doc.RootElement.GetProperty("character_count").GetInt32();
            var limit = doc.RootElement.GetProperty("character_limit").GetInt32();

            settings.SubscriptionCharacterCount = used;
            settings.SubscriptionCharacterLimit = limit;
            _ = _config.SaveAsync(_config.CurrentConfig);

            _log.Info($"ElevenLabs subscription: {used:N0} / {limit:N0} characters used.");
            return (used, limit);
        }
        catch (Exception ex)
        {
            _log.Error("Failed to fetch ElevenLabs subscription info", ex);
            return (0, 0);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to extract a human-readable message from an ElevenLabs JSON error body.
    /// ElevenLabs error bodies are typically <c>{"detail": {"message": "..."}}  </c> or
    /// <c>{"detail": "..."}</c>. Returns null if parsing fails.
    /// </summary>
    private static string? TryParseElevenLabsError(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("detail", out var detail)) return null;
            if (detail.ValueKind == JsonValueKind.String)
                return detail.GetString();
            if (detail.ValueKind == JsonValueKind.Object &&
                detail.TryGetProperty("message", out var msg) &&
                msg.ValueKind == JsonValueKind.String)
                return msg.GetString();
            return null;
        }
        catch { return null; }
    }

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
