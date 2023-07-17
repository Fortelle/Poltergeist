using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class TimeSpanToStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not TimeSpan timespan)
        {
            return null;
        }

        if (parameter is not string format)
        {
            return null;
        }

        return timespan.ToString(format);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
