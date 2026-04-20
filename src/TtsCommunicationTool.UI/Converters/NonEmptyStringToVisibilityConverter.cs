using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Returns Visible when the bound string is non-null and non-empty; Collapsed otherwise.
/// Used to show error messages that only appear when there is text.
/// </summary>
public sealed class NonEmptyStringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
