using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Helpers.Converters;

public class ToIconElementConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str)
        {
            var iconinfo = IconInfo.FromString(str);
            return IconInfoHelper.ConvertToIconElement(iconinfo);
        }
        else if (value is IconInfo iconinfo)
        {
            return IconInfoHelper.ConvertToIconElement(iconinfo);
        }
        
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
