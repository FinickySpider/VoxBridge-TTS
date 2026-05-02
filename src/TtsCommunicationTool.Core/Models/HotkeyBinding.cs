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
        if (!string.IsNullOrEmpty(Key)) parts.Add(FriendlyKeyName(Key));
        return parts.Count > 0 ? string.Join(" + ", parts) : "(none)";
    }

    /// <summary>Maps stored WPF Key enum names (including numbered OEM aliases) to human-readable labels.</summary>
    private static string FriendlyKeyName(string key) => key.ToUpperInvariant() switch
    {
        "OEMTILDE"       or "OEM3"  or "OEM_3"  => "`/~",
        "OEMMINUS"       or "OEM_MINUS"          => "-/_",
        "OEMPLUS"        or "OEM_PLUS"           => "=/+",
        "OEMOPENBRACKETS" or "OEM4"              => "[/{",
        "OEMCLOSEBRACKETS" or "OEM6"             => "]/}",
        "OEMPIPE"        or "OEM5"               => "\\/|",
        "OEMSEMICOLON"   or "OEM1"               => ";/:",
        "OEMQUOTES"      or "OEM7"               => "'/\"",
        "OEMCOMMA"       or "OEM8"               => ",/<",
        "OEMPERIOD"                              => "./>",
        "OEMQUESTION"    or "OEM2"               => "/?",
        "RETURN"                                 => "Enter",
        "BACK"                                   => "Backspace",
        "ESCAPE"                                 => "Esc",
        "PRIOR"                                  => "PgUp",
        "NEXT"                                   => "PgDn",
        "CAPITAL"                                => "CapsLock",
        "NUMLOCK"                                => "NumLock",
        "SCROLL"                                 => "ScrollLock",
        // D0-D9 → bare number
        var k when k.Length == 2 && k[0] == 'D' && char.IsDigit(k[1]) => k[1..],
        _ => key
    };

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
