using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Manages pre-generated TTS audio cache for quick phrases.
/// Audio is generated once when a phrase is created/updated and stored as WAV files.
/// </summary>
public interface IPhraseCacheService
{
    /// <summary>
    /// Gets the cached audio for a phrase, or null if not cached.
    /// </summary>
    PlaybackRequest? GetCachedAudio(string phraseId);

    /// <summary>
    /// Whether a cached audio file exists for this phrase.
    /// </summary>
    bool HasCache(string phraseId);

    /// <summary>
    /// Generates and caches TTS audio for the given phrase.
    /// </summary>
    Task GenerateCacheAsync(PhraseItem phrase);

    /// <summary>
    /// Deletes the cached audio for a phrase.
    /// </summary>
    void DeleteCache(string phraseId);

    /// <summary>
    /// Re-generates cache for all phrases (e.g., after voice change).
    /// </summary>
    Task RegenerateAllAsync(IReadOnlyList<PhraseItem> phrases);
}
