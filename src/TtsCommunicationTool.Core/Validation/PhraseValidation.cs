using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Validation;

public static class PhraseValidation
{
    public const int MaxNameLength = 50;
    public const int MaxTextLength = 500;

    public static (bool IsValid, string? Error) Validate(PhraseItem? phrase)
    {
        if (phrase is null)
            return (false, "Phrase cannot be null.");

        if (string.IsNullOrWhiteSpace(phrase.Name))
            return (false, "Phrase name cannot be empty.");

        if (phrase.Name.Length > MaxNameLength)
            return (false, $"Phrase name exceeds the maximum length of {MaxNameLength} characters.");

        if (string.IsNullOrWhiteSpace(phrase.Text))
            return (false, "Phrase text cannot be empty.");

        if (phrase.Text.Length > MaxTextLength)
            return (false, $"Phrase text exceeds the maximum length of {MaxTextLength} characters.");

        return (true, null);
    }
}
