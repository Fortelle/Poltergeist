using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class IntToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return System.Convert.ToDouble(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return System.Convert.ToInt32(value);
    }
}
