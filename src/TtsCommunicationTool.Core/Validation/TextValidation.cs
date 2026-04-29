namespace TtsCommunicationTool.Core.Validation;

public static class TextValidation
{
    public const int MaxInputLength = 500;

    public static (bool IsValid, string? Error) Validate(string? text)
        => Validate(text, MaxInputLength, true);

    public static (bool IsValid, string? Error) Validate(string? text, int maxLength, bool enforceLimit)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, "Text cannot be empty.");

        if (enforceLimit && maxLength > 0 && text.Length > maxLength)
            return (false, $"Text exceeds the maximum length of {maxLength} characters.");

        return (true, null);
    }

    public static string Sanitize(string text)
    {
        return text.Trim();
    }
}
