using NAudio.Wave;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Tts;

/// <summary>
/// Windows SAPI5 adapter. Synthesis is rendered to an in-memory WAV stream so the
/// shared dual-output audio router remains the only component that talks to devices.
/// </summary>
public sealed class Sapi5TtsService : ITtsService
{
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private IReadOnlyList<VoiceInfo> _voices = Array.Empty<VoiceInfo>();
    private bool _initialized;

    public Sapi5TtsService(IConfigService config, ILoggingService log)
    {
        _config = config;
        _log = log;
    }

    public bool IsInitialized => _initialized;

    public Task InitializeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        RefreshVoices();
        _initialized = true;
        _log.Info($"SAPI5 initialized. {_voices.Count} installed voice(s) found.");
        return Task.CompletedTask;
    }

    public IReadOnlyList<VoiceInfo> GetAvailableVoices()
    {
        if (!_initialized)
            RefreshVoices();
        return _voices;
    }

    public async Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return TtsResult.Fail("Text cannot be empty.");

        try
        {
            var settings = _config.CurrentConfig.Sapi5;
            var voiceId = request.VoiceId ?? string.Empty;
            var rate = Math.Clamp(settings.Rate + SpeedToSapiRate(request.Speed), -10, 10);
            var volume = Math.Clamp(settings.Volume, 0, 100);

            var wavBytes = await Task.Run(() => SynthesizeWave(request.Text, voiceId, rate, volume, ct), ct)
                .ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            using var stream = new MemoryStream(wavBytes, writable: false);
            using var reader = new WaveFileReader(stream);
            var format = reader.WaveFormat;
            var audio = ReadAll(reader);

            if (audio.Length == 0)
                return TtsResult.Fail("SAPI5 generated no audio samples.");

            return TtsResult.Ok(audio, format.SampleRate, format.Channels, format.BitsPerSample);
        }
        catch (OperationCanceledException)
        {
            return TtsResult.Fail("Speech generation was cancelled.");
        }
        catch (Exception ex)
        {
            _log.Error("SAPI5 synthesis failed.", ex);
            return TtsResult.Fail($"SAPI5 speech generation failed: {ex.Message}");
        }
    }

    private byte[] SynthesizeWave(string text, string voiceId, int rate, int volume, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        using var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
        using var output = new MemoryStream();

        if (!string.IsNullOrWhiteSpace(voiceId))
        {
            var installed = synthesizer.GetInstalledVoices()
                .FirstOrDefault(v => string.Equals(v.VoiceInfo.Id, voiceId, StringComparison.OrdinalIgnoreCase));
            if (installed is null || !installed.Enabled)
                throw new InvalidOperationException($"The configured SAPI5 voice is not installed or enabled: {voiceId}");

            synthesizer.SelectVoice(installed.VoiceInfo.Name);
        }

        synthesizer.Rate = rate;
        synthesizer.Volume = volume;
        synthesizer.SetOutputToWaveStream(output);
        synthesizer.Speak(text);
        synthesizer.SetOutputToNull();
        ct.ThrowIfCancellationRequested();
        return output.ToArray();
    }

    private void RefreshVoices()
    {
        try
        {
            using var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
            _voices = synthesizer.GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => new VoiceInfo
                {
                    Id = v.VoiceInfo.Id,
                    DisplayName = v.VoiceInfo.Name,
                    EngineName = "SAPI5"
                })
                .ToArray();
        }
        catch (Exception ex)
        {
            _voices = Array.Empty<VoiceInfo>();
            _log.Error("Unable to enumerate SAPI5 voices.", ex);
        }
    }

    private static int SpeedToSapiRate(float speed)
    {
        var normalized = Math.Clamp(speed, 0.5f, 2.0f);
        return (int)Math.Round(Math.Log(normalized, 2) * 10, MidpointRounding.AwayFromZero);
    }

    private static byte[] ReadAll(WaveStream reader)
    {
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            output.Write(buffer, 0, read);
        return output.ToArray();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
