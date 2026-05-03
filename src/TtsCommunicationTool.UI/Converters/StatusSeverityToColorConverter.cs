using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Converts a <see cref="StatusSeverity"/> value to a <see cref="SolidColorBrush"/>
/// used by the overlay status bar text.
/// </summary>
[ValueConversion(typeof(StatusSeverity), typeof(SolidColorBrush))]
public sealed class StatusSeverityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is StatusSeverity s ? GetBrush(s) : Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;

    private static SolidColorBrush GetBrush(StatusSeverity severity) => severity switch
    {
        StatusSeverity.Success => new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)), // green
        StatusSeverity.Error   => new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)), // red
        StatusSeverity.Warning => new SolidColorBrush(Color.FromRgb(0xFA, 0xB3, 0x87)), // orange
        StatusSeverity.Info    => new SolidColorBrush(Color.FromRgb(0x89, 0xB4, 0xFA)), // blue
        _                      => new SolidColorBrush(Color.FromRgb(0x6C, 0x70, 0x86)), // muted
    };
}
