using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Phrases;

/// <summary>
/// Caches pre-generated TTS audio for quick phrases as WAV files on disk.
/// Files stored at %AppData%\TtsCommunicationTool\phrase_cache\{phraseId}.wav
/// </summary>
public sealed class PhraseCacheService : IPhraseCacheService
{
    private static readonly string CacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool", "phrase_cache");

    // WAV header constants
    private const int WavHeaderSize = 44;

    private readonly ITtsService _tts;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;

    public PhraseCacheService(ITtsService tts, IConfigService config, ILoggingService log)
    {
        _tts = tts;
        _config = config;
        _log = log;
        Directory.CreateDirectory(CacheDir);
    }

    public PlaybackRequest? GetCachedAudio(string phraseId)
    {
        var path = GetCachePath(phraseId);
        if (!File.Exists(path))
            return null;

        try
        {
            var wavBytes = File.ReadAllBytes(path);
            if (wavBytes.Length < WavHeaderSize)
                return null;

            // Parse WAV header for format info
            var channels = BitConverter.ToInt16(wavBytes, 22);
            var sampleRate = BitConverter.ToInt32(wavBytes, 24);
            var bitsPerSample = BitConverter.ToInt16(wavBytes, 34);
            var dataSize = BitConverter.ToInt32(wavBytes, 40);

            // Extract raw PCM data (skip 44-byte header)
            var audioData = new byte[dataSize];
            Array.Copy(wavBytes, WavHeaderSize, audioData, 0, Math.Min(dataSize, wavBytes.Length - WavHeaderSize));

            return new PlaybackRequest
            {
                AudioData = audioData,
                SampleRate = sampleRate,
                Channels = channels,
                BitsPerSample = bitsPerSample
            };
        }
        catch (Exception ex)
        {
            _log.Warn($"Failed to read cached audio for phrase {phraseId}: {ex.Message}");
            return null;
        }
    }

    public bool HasCache(string phraseId) => File.Exists(GetCachePath(phraseId));

    public async Task GenerateCacheAsync(PhraseItem phrase)
    {
        try
        {
            if (!_tts.IsInitialized)
            {
                _log.Warn($"Cannot cache phrase '{phrase.Name}': TTS not initialized.");
                return;
            }

            var voiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId;
            var result = await _tts.SynthesizeAsync(new TtsRequest
            {
                Text = phrase.Text,
                VoiceId = voiceId
            });

            if (!result.Success || result.AudioData is null)
            {
                _log.Warn($"Failed to generate cache for phrase '{phrase.Name}': {result.ErrorMessage}");
                return;
            }

            var wavBytes = CreateWavFile(result.AudioData, result.SampleRate, result.Channels, result.BitsPerSample);
            var path = GetCachePath(phrase.Id);
            await File.WriteAllBytesAsync(path, wavBytes);
            _log.Info($"Cached audio for phrase '{phrase.Name}' ({result.AudioData.Length} bytes).");
        }
        catch (Exception ex)
        {
            _log.Error($"Error caching phrase '{phrase.Name}'", ex);
        }
    }

    public void DeleteCache(string phraseId)
    {
        var path = GetCachePath(phraseId);
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
                _log.Info($"Deleted cached audio for phrase {phraseId}.");
            }
            catch (Exception ex)
            {
                _log.Warn($"Failed to delete cache for phrase {phraseId}: {ex.Message}");
            }
        }
    }

    public async Task RegenerateAllAsync(IReadOnlyList<PhraseItem> phrases)
    {
        _log.Info($"Regenerating cache for {phrases.Count} phrases...");
        foreach (var phrase in phrases)
        {
            await GenerateCacheAsync(phrase);
        }
        _log.Info("Phrase cache regeneration complete.");
    }

    private static string GetCachePath(string phraseId) => Path.Combine(CacheDir, $"{phraseId}.wav");

    private static byte[] CreateWavFile(byte[] pcmData, int sampleRate, int channels, int bitsPerSample)
    {
        var byteRate = sampleRate * channels * (bitsPerSample / 8);
        var blockAlign = (short)(channels * (bitsPerSample / 8));

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + pcmData.Length);
        writer.Write("WAVE"u8);

        // fmt sub-chunk
        writer.Write("fmt "u8);
        writer.Write(16); // sub-chunk size
        writer.Write((short)1); // PCM format
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write((short)bitsPerSample);

        // data sub-chunk
        writer.Write("data"u8);
        writer.Write(pcmData.Length);
        writer.Write(pcmData);

        return ms.ToArray();
    }
}
