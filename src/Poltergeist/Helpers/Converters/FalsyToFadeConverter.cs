using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Helpers.Converters;

public class FalsyToFadeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isTruthy = LogicalUtil.IsTruthy(value);

        return isTruthy ? 1 : 0.5;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}