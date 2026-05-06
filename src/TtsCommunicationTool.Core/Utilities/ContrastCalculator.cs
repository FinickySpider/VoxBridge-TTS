using System.Globalization;

namespace TtsCommunicationTool.Core.Utilities;

/// <summary>
/// Pure-math WCAG 2.1 contrast ratio calculator.
/// No WPF layout dependencies — fully unit-testable.
/// </summary>
public static class ContrastCalculator
{
    /// <summary>
    /// Returns the WCAG 2.1 contrast ratio between two hex colour strings.
    /// Returns 1.0 on parse failure (worst-case ratio).
    /// </summary>
    public static double GetRatio(string hex1, string hex2)
    {
        if (!TryParse(hex1, out double l1) || !TryParse(hex2, out double l2))
            return 1.0;

        // Ensure L1 >= L2
        if (l1 < l2) (l1, l2) = (l2, l1);
        return (l1 + 0.05) / (l2 + 0.05);
    }

    /// <summary>
    /// Returns a human-readable status for the ratio against WCAG 2.1 AA thresholds:
    /// "Good" (≥4.5), "Warning" (3.0–4.49), or "Fail" (&lt;3.0).
    /// </summary>
    public static string GetStatus(double ratio) =>
        ratio >= 4.5 ? "Good" :
        ratio >= 3.0 ? "Warning" :
        "Fail";

    // ── Private helpers ───────────────────────────────────────────────────────

    private static bool TryParse(string hex, out double luminance)
    {
        luminance = 0;
        try
        {
            var s = hex.TrimStart('#');
            if (s.Length != 6) return false;
            byte r = byte.Parse(s[0..2], NumberStyles.HexNumber);
            byte g = byte.Parse(s[2..4], NumberStyles.HexNumber);
            byte b = byte.Parse(s[4..6], NumberStyles.HexNumber);
            luminance = RelativeLuminance(r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static double RelativeLuminance(byte r, byte g, byte b)
    {
        double rL = Linearise(r / 255.0);
        double gL = Linearise(g / 255.0);
        double bL = Linearise(b / 255.0);
        return 0.2126 * rL + 0.7152 * gL + 0.0722 * bL;
    }

    private static double Linearise(double c) =>
        c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
}
