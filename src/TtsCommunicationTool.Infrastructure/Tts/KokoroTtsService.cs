using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Processing;
using System.Diagnostics;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Logging;

namespace TtsCommunicationTool.Infrastructure.Tts;

/// <summary>
/// Real Kokoro TTS service using KokoroSharp for offline speech synthesis.
/// Model (~320MB) is auto-downloaded on first use.
/// </summary>
public sealed class KokoroTtsService : ITtsService, IDisposable
{
    private const int SampleRate = 24000;  // Kokoro native sample rate
    private const string DefaultVoiceId = "af_heart";

    private readonly ILoggingService _log;
    private KokoroTTS? _tts;
    private bool _disposed;

    public bool IsInitialized => _tts is not null;

    public KokoroTtsService(ILoggingService log)
    {
        _log = log;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_tts is not null) return;

        _log.Info("Initializing Kokoro TTS engine (model will auto-download on first run ~320MB)...");

        try
        {
            // LoadModel downloads the ONNX model if not cached and loads it.
            // Run on thread pool to avoid blocking UI.
            _tts = await Task.Run(() => KokoroTTS.LoadModel(), ct);

            // Explicitly load voice files so GetAvailableVoices() works immediately.
            // GetVoice() lazy-loads them, but Voices list is empty until first access.
            await Task.Run(() => KokoroVoiceManager.LoadVoicesFromPath(), ct);

            _log.Info($"Kokoro TTS engine initialized successfully. {KokoroVoiceManager.Voices.Count} voices loaded.");
        }
        catch (Exception ex)
        {
            _log.Error("Failed to initialize Kokoro TTS engine.", ex);
            throw;
        }
    }

    public async Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        if (_tts is null)
            return TtsResult.Fail("TTS engine is not initialized.");

        if (string.IsNullOrWhiteSpace(request.Text))
            return TtsResult.Fail("Text cannot be empty.");

        var voiceId = string.IsNullOrEmpty(request.VoiceId) ? DefaultVoiceId : request.VoiceId;
        var sw = Stopwatch.StartNew();

        _log.LogEvent(DiagnosticLogLevel.Info, "tts", "synthesis_started",
            "Kokoro TTS synthesis started",
            new
            {
                voice_id   = voiceId,
                engine     = "kokoro",
                text_length = request.Text.Length,
                text_hash   = FileLoggingService.ComputeTextHash(request.Text),
                text        = _log.LogRawText ? request.Text : (string?)null,
                request_id  = _log.IncludeRequestIds ? request.RequestId : null
            });

        try
        {
            var voice = KokoroVoiceManager.GetVoice(voiceId);
            if (voice is null)
            {
                _log.Warn($"Voice '{voiceId}' not found, falling back to '{DefaultVoiceId}'.");
                voice = KokoroVoiceManager.GetVoice(DefaultVoiceId);
                if (voice is null)
                    return TtsResult.Fail($"Default voice '{DefaultVoiceId}' not found.");
            }

            // Tokenize and segment for faster first-response (same approach as SpeakFast)
            var allSamples = await Task.Run(() => GenerateSamples(request.Text, voice), ct);

            if (allSamples.Length == 0)
                return TtsResult.Fail("TTS generated no audio samples.");

            // Convert float samples to 16-bit PCM bytes
            var audioBytes = ConvertToPcm16(allSamples);
            _log.Info($"Generated {audioBytes.Length} bytes of audio ({allSamples.Length} samples at {SampleRate}Hz).");

            sw.Stop();
            _log.LogEvent(DiagnosticLogLevel.Info, "tts", "synthesis_completed",
                "Kokoro TTS synthesis completed",
                new
                {
                    voice_id    = voiceId,
                    engine      = "kokoro",
                    duration_ms = (long)sw.Elapsed.TotalMilliseconds,
                    text_length = request.Text.Length,
                    request_id  = _log.IncludeRequestIds ? request.RequestId : null
                });

            // Apply global pitch by adjusting the declared sample rate.
            // Higher declared rate → audio plays faster/higher-pitched; lower → slower/lower.
            // Adjusting the declared sample rate changes the generated audio's playback
            // speed without rewriting the PCM buffer. The same approach is used for pitch;
            // combine both multipliers so each setting remains independently persisted.
            var playbackRate = Math.Clamp(request.Pitch, 0.5f, 2.0f)
                              * Math.Clamp(request.Speed, 0.5f, 2.0f);
            var playbackSampleRate = (int)(SampleRate * playbackRate);
            return TtsResult.Ok(audioBytes, playbackSampleRate, channels: 1, bitsPerSample: 16);
        }
        catch (OperationCanceledException)
        {
            _log.LogEvent(DiagnosticLogLevel.Info, "tts", "synthesis_failed",
                "TTS synthesis cancelled",
                new { voice_id = voiceId, engine = "kokoro", request_id = _log.IncludeRequestIds ? request.RequestId : null });
            return TtsResult.Fail("Speech generation was cancelled.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.Error("Kokoro TTS synthesis failed.", ex);
            _log.LogEvent(DiagnosticLogLevel.Error, "tts", "synthesis_failed",
                "Kokoro TTS synthesis failed",
                new { voice_id = voiceId, engine = "kokoro", error = ex.Message, request_id = _log.IncludeRequestIds ? request.RequestId : null });
            return TtsResult.Fail($"Speech generation failed: {ex.Message}");
        }
    }

    public IReadOnlyList<VoiceInfo> GetAvailableVoices()
    {
        var voices = new List<VoiceInfo>();
        foreach (var voice in KokoroVoiceManager.Voices)
        {
            voices.Add(new VoiceInfo
            {
                Id = voice.Name,
                DisplayName = voice.Name,
                EngineName = "Kokoro"
            });
        }
        return voices.AsReadOnly();
    }

    private float[] GenerateSamples(string text, KokoroVoice voice)
    {
        // Tokenize the text into phoneme tokens
        var tokens = Tokenizer.Tokenize(text);
        if (tokens.Length == 0) return [];

        // Split into segments for faster response
        var segments = SegmentationSystem.SplitToSegments(tokens, new DefaultSegmentationConfig
        {
            MaxFirstSegmentLength = 100
        });

        // Collect all samples from each segment
        var allSamples = new List<float>();
        var tcs = new TaskCompletionSource();
        var remaining = segments.Count;

        foreach (var segment in segments)
        {
            var job = KokoroJob.Create(segment, voice, speed: 1f, samples =>
            {
                lock (allSamples)
                {
                    allSamples.AddRange(samples);
                }
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    tcs.TrySetResult();
                }
            });
            _tts!.EnqueueJob(job);
        }

        // Wait for all segments to complete
        tcs.Task.Wait(TimeSpan.FromSeconds(30));

        return allSamples.ToArray();
    }

    private static byte[] ConvertToPcm16(float[] samples)
    {
        var bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            // Clamp to [-1, 1] and convert to 16-bit signed integer
            var clamped = Math.Clamp(samples[i], -1f, 1f);
            var value = (short)(clamped * short.MaxValue);
            bytes[i * 2] = (byte)(value & 0xFF);
            bytes[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }
        return bytes;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _tts?.Dispose();
        _tts = null;
        _log.Info("Kokoro TTS engine disposed.");
    }
}
