using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Poltergeist.Common.Converters;

public class StringToSolidColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value as string;
        var brush = (SolidColorBrush)new BrushConverter().ConvertFrom(str);
        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}