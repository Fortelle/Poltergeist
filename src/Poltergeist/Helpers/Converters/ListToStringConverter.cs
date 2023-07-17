using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class ListToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
        throw new NotSupportedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string text)
        {
            return text.Split("\n");
        }
        throw new NotSupportedException();
    }
}
