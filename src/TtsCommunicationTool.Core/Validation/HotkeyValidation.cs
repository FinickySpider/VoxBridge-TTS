using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Validation;

public static class HotkeyValidation
{
    private static readonly HashSet<string> ReservedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "LeftCtrl", "RightCtrl", "LeftAlt", "RightAlt",
        "LeftShift", "RightShift", "LWin", "RWin"
    };

    public static (bool IsValid, string? Error) Validate(HotkeyBinding? binding)
    {
        if (binding is null)
            return (false, "Hotkey binding cannot be null.");

        if (string.IsNullOrWhiteSpace(binding.Key))
            return (false, "Hotkey must have a key assigned.");

        if (ReservedKeys.Contains(binding.Key))
            return (false, $"'{binding.Key}' cannot be used as a hotkey by itself.");

        if (!binding.Ctrl && !binding.Alt && !binding.Shift && !binding.Win)
            return (false, "Hotkey must include at least one modifier (Ctrl, Alt, or Shift).");

        if (binding.Win)
            return (false, "Win key is not supported as a hotkey modifier.");

        return (true, null);
    }

    public static bool AreConflicting(HotkeyBinding a, HotkeyBinding b)
    {
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
}
