using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Returns Visible when the bound string is null or empty; Collapsed otherwise.
/// Used to show placeholder/watermark text over empty text boxes.
/// </summary>
public sealed class EmptyStringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
