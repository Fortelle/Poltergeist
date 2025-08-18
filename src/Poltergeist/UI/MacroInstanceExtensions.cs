using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Components.Thumbnails;
using Poltergeist.Automations.Structures;
using Poltergeist.Helpers;
using Poltergeist.Modules.Macros;

namespace Poltergeist.UI;

public static partial class MacroInstanceExtensions
{
    public static IconSource? GetIconSource(this MacroInstance instance)
    {
        if (instance.Icon is not null)
        {
            var info = new IconInfo(instance.Icon);
            var source = IconInfoHelper.ConvertToIconSource(info);
            if (source is not null)
            {
                return source;
            }
        }
        return IconInfoHelper.ConvertToIconSource(new IconInfo(MacroInstanceManager.DefaultIconUri));
    }

    public static IconElement? GetIconElement(this MacroInstance instance)
    {
        if (instance.Icon is not null)
        {
            var info = new IconInfo(instance.Icon);
            var element = IconInfoHelper.ConvertToIconElement(info);
            if (element is not null)
            {
                return element;
            }
        }
        return IconInfoHelper.ConvertToIconElement(new IconInfo(MacroInstanceManager.DefaultIconUri));
    }

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
