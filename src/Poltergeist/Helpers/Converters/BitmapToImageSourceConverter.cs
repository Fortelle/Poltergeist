using System.Drawing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Poltergeist.Helpers.Converters;

public class BitmapToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Bitmap bmp)
        {
            return null;
        }

        return BitmapHelper.ToImageSource(bmp);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
