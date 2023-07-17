using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class TypeNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.GetType().Name ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
