using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Helpers.Converters;

public class FalsyToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isTruthy = LogicalUtil.IsTruthy(value);

        return isTruthy ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}
