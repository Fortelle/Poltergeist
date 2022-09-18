using System;
using System.Globalization;
using System.Windows.Data;

namespace Poltergeist.Common.Converters;

public class ListToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
        throw new NotSupportedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return text.Split("\n");
        }
        throw new NotSupportedException();
    }
}
