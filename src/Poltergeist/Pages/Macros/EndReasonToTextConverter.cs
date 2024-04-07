using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Pages.Macros;

internal class EndReasonToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return App.Localize($"Poltergeist/Macros/EndReason_{value}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}