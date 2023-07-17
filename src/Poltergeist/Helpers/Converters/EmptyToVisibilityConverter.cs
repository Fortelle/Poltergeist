using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class EmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var visible = value switch
        {
            null => false,
            bool b => b,
            int i => i > 0,
            IEnumerable ie => ie.GetEnumerator().MoveNext(),
            _ => true,
        };

        if (parameter is bool reverse && reverse)
        {
            visible = !visible;
        }
        else if (parameter is string s && s == "True")
        {
            visible = !visible;
        }
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}
