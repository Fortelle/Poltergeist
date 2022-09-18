using System;
using System.Globalization;
using System.Windows.Data;

namespace Poltergeist.Common.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public EnumToBooleanConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString)
        {
            var enumValue = Enum.Parse(targetType, enumString);

            return enumValue.Equals(value);
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
