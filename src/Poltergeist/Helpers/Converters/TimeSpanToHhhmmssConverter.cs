using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class TimeSpanToHhhmmssConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not TimeSpan timespan)
        {
            return null;
        }

        return ToString(timespan);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public static string ToString(TimeSpan timespan)
    {
        return $"{timespan.TotalHours:00}:{timespan.Minutes:00}:{timespan.Seconds:00}";
    }

}
