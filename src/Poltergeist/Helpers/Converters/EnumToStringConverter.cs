using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is null ? string.Empty : value.ToString() ?? "";

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}
