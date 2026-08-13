using System.Speech.Synthesis;
using System.Text.Json;

namespace TtsCommunicationTool.Sapi5Bridge;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var input = await Console.In.ReadToEndAsync().ConfigureAwait(false);
            var request = JsonSerializer.Deserialize<BridgeRequest>(input, JsonOptions)
                ?? throw new InvalidOperationException("The bridge request was empty.");

            var response = request.Operation switch
            {
                "enumerate" => Enumerate(),
                "synthesize" => Synthesize(request),
                _ => throw new InvalidOperationException($"Unknown bridge operation: {request.Operation}")
            };

            Console.Out.Write(JsonSerializer.Serialize(response, JsonOptions));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Out.Write(JsonSerializer.Serialize(new BridgeResponse
            {
                Success = false,
                Error = ex.Message
            }, JsonOptions));
            return 1;
        }
    }

    private static BridgeResponse Enumerate()
    {
        using var synthesizer = new SpeechSynthesizer();
        return new BridgeResponse
        {
            Success = true,
            Voices = synthesizer.GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => new BridgeVoice
                {
                    Id = v.VoiceInfo.Id,
                    Name = v.VoiceInfo.Name
                })
                .ToArray()
        };
    }

    private static BridgeResponse Synthesize(BridgeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Text cannot be empty.");

        using var synthesizer = new SpeechSynthesizer();
        if (!string.IsNullOrWhiteSpace(request.VoiceId))
        {
            var voice = synthesizer.GetInstalledVoices()
                .FirstOrDefault(v => string.Equals(v.VoiceInfo.Id, request.VoiceId, StringComparison.OrdinalIgnoreCase));
            if (voice is null || !voice.Enabled)
                throw new InvalidOperationException($"The configured SAPI5 voice is not installed or enabled: {request.VoiceId}");
            synthesizer.SelectVoice(voice.VoiceInfo.Name);
        }

        synthesizer.Rate = Math.Clamp(request.Rate, -10, 10);
        synthesizer.Volume = Math.Clamp(request.Volume, 0, 100);
        using var output = new MemoryStream();
        synthesizer.SetOutputToWaveStream(output);
        synthesizer.Speak(request.Text);
        synthesizer.SetOutputToNull();

        return new BridgeResponse
        {
            Success = true,
            AudioBase64 = Convert.ToBase64String(output.ToArray())
        };
    }

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
