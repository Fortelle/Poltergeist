using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Helpers.Converters;

public class FalsyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var visible = LogicalUtil.IsTruthy(value);

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
