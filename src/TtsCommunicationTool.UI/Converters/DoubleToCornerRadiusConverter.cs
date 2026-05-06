using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TtsCommunicationTool.UI.Converters;

/// <summary>
/// Converts a <see cref="double"/> (or <see cref="int"/>) value into a uniform
/// <see cref="CornerRadius"/> so XAML preview borders can bind to ThemeCornerRadius.
/// </summary>
[ValueConversion(typeof(double), typeof(CornerRadius))]
public class DoubleToCornerRadiusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d) return new CornerRadius(Math.Max(0, d));
        if (value is int i)    return new CornerRadius(Math.Max(0, i));
        if (value is float f)  return new CornerRadius(Math.Max(0, f));
        return new CornerRadius(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
