using System.Diagnostics;
using System.Text.Json;
using NAudio.Wave;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Tts;

/// <summary>SAPI5 adapter backed by an x86 helper process for 32-bit-only voices.</summary>
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

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            _voices = await EnumerateVoicesAsync(ct).ConfigureAwait(false);
            _initialized = true;
            _log.Info($"SAPI5 initialized through x86 bridge. {_voices.Count} installed voice(s) found.");
        }
        catch (Exception ex)
        {
            _voices = Array.Empty<VoiceInfo>();
            _initialized = true;
            _log.Error("Unable to initialize the x86 SAPI5 bridge.", ex);
        }
    }

    public IReadOnlyList<VoiceInfo> GetAvailableVoices() => _voices;

    public async Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return TtsResult.Fail("Text cannot be empty.");

        try
        {
            var settings = _config.CurrentConfig.Sapi5;
            var response = await RunBridgeAsync(new BridgeRequest
            {
                Operation = "synthesize",
                Text = request.Text,
                VoiceId = request.VoiceId ?? string.Empty,
                Rate = Math.Clamp(settings.Rate + SpeedToSapiRate(request.Speed), -10, 10),
                Volume = Math.Clamp(settings.Volume, 0, 100)
            }, ct).ConfigureAwait(false);

            if (!response.Success || string.IsNullOrWhiteSpace(response.AudioBase64))
                return TtsResult.Fail(response.Error ?? "The SAPI5 bridge generated no audio.");

            using var stream = new MemoryStream(Convert.FromBase64String(response.AudioBase64), writable: false);
            using var reader = new WaveFileReader(stream);
            var format = reader.WaveFormat;
            var audio = ReadAll(reader);
            return audio.Length == 0
                ? TtsResult.Fail("SAPI5 generated no audio samples.")
                : TtsResult.Ok(audio, format.SampleRate, format.Channels, format.BitsPerSample);
        }
        catch (OperationCanceledException)
        {
            return TtsResult.Fail("Speech generation was cancelled.");
        }
        catch (Exception ex)
        {
            _log.Error("SAPI5 bridge synthesis failed.", ex);
            return TtsResult.Fail($"SAPI5 speech generation failed: {ex.Message}");
        }
    }

    private async Task<IReadOnlyList<VoiceInfo>> EnumerateVoicesAsync(CancellationToken ct)
    {
        var response = await RunBridgeAsync(new BridgeRequest { Operation = "enumerate" }, ct).ConfigureAwait(false);
        if (!response.Success)
            throw new InvalidOperationException(response.Error ?? "The SAPI5 bridge could not enumerate voices.");

        return (response.Voices ?? Array.Empty<BridgeVoice>())
            .Select(v => new VoiceInfo { Id = v.Id, DisplayName = v.Name, EngineName = "SAPI5" })
            .ToArray();
    }

    private async Task<BridgeResponse> RunBridgeAsync(BridgeRequest request, CancellationToken ct)
    {
        var bridgePath = Path.Combine(AppContext.BaseDirectory, "TtsCommunicationTool.Sapi5Bridge.exe");
        if (!File.Exists(bridgePath))
            throw new FileNotFoundException("The x86 SAPI5 bridge executable was not found.", bridgePath);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = bridgePath,
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        if (!process.Start())
            throw new InvalidOperationException("The x86 SAPI5 bridge process could not be started.");

        try
        {
            await process.StandardInput.WriteAsync(JsonSerializer.Serialize(request)).ConfigureAwait(false);
            process.StandardInput.Close();
            var outputTask = process.StandardOutput.ReadToEndAsync(ct);
            var errorTask = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct).ConfigureAwait(false);
            var output = await outputTask.ConfigureAwait(false);
            var error = await errorTask.ConfigureAwait(false);

            if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                    ? $"The x86 SAPI5 bridge exited with code {process.ExitCode}."
                    : error.Trim());

            return JsonSerializer.Deserialize<BridgeResponse>(output)
                ?? throw new InvalidOperationException("The x86 SAPI5 bridge returned an empty response.");
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            throw;
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

    private sealed class BridgeRequest
    {
        public string Operation { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public string VoiceId { get; init; } = string.Empty;
        public int Rate { get; init; }
        public int Volume { get; init; } = 100;
    }

    private sealed class BridgeResponse
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public BridgeVoice[]? Voices { get; init; }
        public string? AudioBase64 { get; init; }
    }

    private sealed class BridgeVoice
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
