using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Helpers.Converters;

public class IsTruthyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return LogicalUtil.IsTruthy(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
