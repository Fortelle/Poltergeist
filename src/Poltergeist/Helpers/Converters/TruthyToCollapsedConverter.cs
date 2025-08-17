using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Helpers.Converters;

public class TruthyToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isTruthy = LogicalUtil.IsTruthy(value);

        return isTruthy ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
