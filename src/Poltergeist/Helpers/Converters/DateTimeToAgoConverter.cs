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
            { TotalMinutes: <= 1 } => App.Localize("Poltergeist/Home/DateTimeToAgo_1"),
            { TotalMinutes: < 60 } => App.Localize("Poltergeist/Home/DateTimeToAgo_2", timespan.TotalMinutes),
            { TotalHours: <= 1 } => App.Localize("Poltergeist/Home/DateTimeToAgo_3"),
            { TotalHours: < 24 } => App.Localize("Poltergeist/Home/DateTimeToAgo_4", timespan.TotalHours),
            { TotalDays: <= 1 } => App.Localize("Poltergeist/Home/DateTimeToAgo_5"),
            { TotalDays: < 30 } => App.Localize("Poltergeist/Home/DateTimeToAgo_6", timespan.TotalDays),
            _ => datetime.ToLocalTime(),
        };
    }


    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
