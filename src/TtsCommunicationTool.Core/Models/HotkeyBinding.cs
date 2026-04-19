namespace TtsCommunicationTool.Core.Models;

public sealed class HotkeyBinding
{
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }
    public string Key { get; set; } = string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(Key);

    public override string ToString()
    {
        var parts = new List<string>();
        if (Ctrl) parts.Add("Ctrl");
        if (Alt) parts.Add("Alt");
        if (Shift) parts.Add("Shift");
        if (Win) parts.Add("Win");
        if (!string.IsNullOrEmpty(Key)) parts.Add(Key);
        return parts.Count > 0 ? string.Join(" + ", parts) : "(none)";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not HotkeyBinding other) return false;
        return Ctrl == other.Ctrl
            && Alt == other.Alt
            && Shift == other.Shift
            && Win == other.Win
            && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
        => HashCode.Combine(Ctrl, Alt, Shift, Win, Key?.ToLowerInvariant());

    public HotkeyBinding Clone() => new()
    {
        Ctrl = Ctrl,
        Alt = Alt,
        Shift = Shift,
        Win = Win,
        Key = Key
    };
}
