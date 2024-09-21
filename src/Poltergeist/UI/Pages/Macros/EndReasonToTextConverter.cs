using Microsoft.UI.Xaml.Data;

namespace Poltergeist.UI.Pages.Macros;

internal class EndReasonToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return PoltergeistApplication.Localize($"Poltergeist/Macros/EndReason_{value}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}