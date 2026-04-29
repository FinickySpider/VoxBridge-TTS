namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Applies ordered text replacement rules single-pass before TTS synthesis.
/// </summary>
public interface ITextReplacementService
{
    /// <summary>
    /// Applies all enabled replacement rules to <paramref name="input"/> and
    /// returns the transformed string. Single-pass: already-replaced spans
    /// are never re-scanned.
    /// </summary>
    string Apply(string input);
}
