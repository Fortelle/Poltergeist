using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.UI.Pages.Logging;

public class LogLevelToIconConverter : IValueConverter
{
    private static readonly Dictionary<LogLevel, string> Glyphs = new()
    {
        [LogLevel.Trace] = "\uE81B",
        [LogLevel.Debug] = "\uEBE8",
        [LogLevel.Warning] = "\uE7BA",
        [LogLevel.Error] = "\uEA39",
        [LogLevel.Critical] = "\uEB90",
        [LogLevel.None] = "\uE946",
    };

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return Glyphs.TryGetValue((LogLevel)value, out var icons) ? icons : Glyphs[LogLevel.None];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
