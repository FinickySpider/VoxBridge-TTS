using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Converts a WCAG contrast status string ("Good", "Warning", "Fail") to a
/// background brush for the status chip in the Theme tab Contrast section.
/// </summary>
[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public sealed class ContrastStatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush GoodBrush    = new(Color.FromRgb(0x40, 0xA0, 0x60));
    private static readonly SolidColorBrush WarningBrush = new(Color.FromRgb(0xD0, 0x9A, 0x20));
    private static readonly SolidColorBrush FailBrush    = new(Color.FromRgb(0xC0, 0x3A, 0x3A));
    private static readonly SolidColorBrush FallbackBrush= new(Color.FromRgb(0x55, 0x57, 0x70));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "Good"    => GoodBrush,
            "Warning" => WarningBrush,
            "Fail"    => FailBrush,
            _         => FallbackBrush,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
