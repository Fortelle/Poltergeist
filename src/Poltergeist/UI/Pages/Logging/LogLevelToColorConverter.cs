using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.UI.Pages.Logging;

public class LogLevelToColorConverter : IValueConverter
{
    private static readonly Dictionary<LogLevel, SolidColorBrush> Brushes = new()
    {
        [LogLevel.Trace] = new(Colors.DeepSkyBlue),
        [LogLevel.Debug] = new(Colors.DarkGray),
        [LogLevel.Warning] = new(Colors.DarkOrange),
        [LogLevel.Error] = new(Colors.Red),
        [LogLevel.Critical] = new(Colors.Red),
        [LogLevel.None] = new(Colors.Black),
    };

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return Brushes.TryGetValue((LogLevel)value, out var brush) ? brush : Brushes[LogLevel.None];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
