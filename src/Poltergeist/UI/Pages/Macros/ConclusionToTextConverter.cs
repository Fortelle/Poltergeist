using Microsoft.UI.Xaml.Data;

namespace Poltergeist.UI.Pages.Macros;

internal class ConclusionToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return PoltergeistApplication.Localize($"Poltergeist/Macros/Conclusion_{value}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}