namespace TtsCommunicationTool.Core.Validation;

public static class TextValidation
{
    public const int MaxInputLength = 500;

    public static (bool IsValid, string? Error) Validate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, "Text cannot be empty.");

        if (text.Length > MaxInputLength)
            return (false, $"Text exceeds the maximum length of {MaxInputLength} characters.");

        return (true, null);
    }

    public static string Sanitize(string text)
    {
        return text.Trim();
    }
}
