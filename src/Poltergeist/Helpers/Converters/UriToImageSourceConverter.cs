using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Poltergeist.Helpers.Converters;

public class UriToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string uriString)
        {
            var uri = new Uri(uriString);
            return new BitmapImage(uri);
        }
        else if (value is Uri uri)
        {
            return new BitmapImage(uri);
        }

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}