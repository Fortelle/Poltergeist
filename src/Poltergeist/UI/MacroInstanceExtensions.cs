using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Components.Thumbnails;
using Poltergeist.Modules.Macros;

namespace Poltergeist.UI;

public static partial class MacroInstanceExtensions
{
    public static BitmapImage? GetThumbnail(this MacroInstance instance)
    {
        if (instance.PrivateFolder is null)
        {
            return null;
        }

        var thumbnailPath = Path.Combine(instance.PrivateFolder, ThumbnailExtensions.ThumbnailFilename);
        if (!File.Exists(thumbnailPath))
        {
            return null;
        }

        try
        {
            var uri = new Uri(thumbnailPath);
            var bmp = new BitmapImage(uri);
            return bmp;
        }
        catch
        {
        }

        return null;
    }

}
