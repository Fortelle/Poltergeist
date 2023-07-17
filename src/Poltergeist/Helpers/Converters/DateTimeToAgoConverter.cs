using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class DateTimeToAgoConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DateTime datetime)
        {
            return null;
        }

        var timespan = DateTime.Now - datetime;
        return timespan switch
        {
            { TotalMinutes: <= 1 } => $"a minute ago",
            { TotalMinutes: < 60 } => $"{timespan.TotalMinutes:#} minutes ago",
            { TotalHours: <= 1 } => $"an hour ago",
            { TotalHours: < 24 } => $"{timespan.TotalHours:#} hours ago",
            { TotalDays: <= 1 } => $"yesterday",
            { TotalDays: < 30 } => $"{timespan.TotalDays:#} days ago",
            _ => datetime.ToLocalTime(),
        };
    }


    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
