using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class NotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not bool b || !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is not bool b || !b;
    }
}
