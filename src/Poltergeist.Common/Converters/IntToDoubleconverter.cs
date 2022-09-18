using System;
using System.Globalization;
using System.Windows.Data;

namespace Poltergeist.Common.Converters;

public class IntToDoubleconverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Convert.ToDouble(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Convert.ToInt32(value);
    }
}
