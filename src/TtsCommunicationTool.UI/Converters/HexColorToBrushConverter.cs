using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Converts a "#RRGGBB" or "#AARRGGBB" hex string to a <see cref="SolidColorBrush"/>.
/// Returns <see cref="Brushes.Transparent"/> for null, empty, or unparseable input so the
/// swatch stays visible without throwing.
/// </summary>
[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public sealed class HexColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch { /* fall through */ }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
