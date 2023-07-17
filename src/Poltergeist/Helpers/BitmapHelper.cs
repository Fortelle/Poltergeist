using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Poltergeist.Helpers;

public class BitmapHelper
{
    public static ImageSource ToImageSource(Bitmap bitmap)
    {
        var bitmapImage = new BitmapImage();
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            bitmapImage.SetSource(stream.AsRandomAccessStream());
        }
        return bitmapImage;
    }

}
