using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Converts a HotkeyBinding? to a display string (e.g. "Ctrl+Shift+F1") or "(none)" if null.
/// </summary>
public sealed class HotkeyBindingDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not HotkeyBinding hk)
            return "(none)";

        var parts = new List<string>();
        if (hk.Ctrl) parts.Add("Ctrl");
        if (hk.Alt) parts.Add("Alt");
        if (hk.Shift) parts.Add("Shift");
        if (hk.Win) parts.Add("Win");
        if (!string.IsNullOrEmpty(hk.Key)) parts.Add(hk.Key);

        return parts.Count > 0 ? string.Join("+", parts) : "(none)";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
