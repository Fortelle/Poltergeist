using System.IO;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Components.Thumbnails;

public static class ThumbnailExtensions
{
    private const string ThumbnailFilename = "thumbnail.png";

    public static string? GetThumbnailFile(this IMacroBase macro)
    {
        if (macro.PrivateFolder is null)
        {
            return null;
        }

        var path = Path.Combine(macro.PrivateFolder, ThumbnailFilename);

        if (!File.Exists(path))
        {
            return null;
        }

        return path;
    }

    public static void SetThumbnailFile(this IMacroBase macro, System.Drawing.Bitmap image)
    {
        if (macro.PrivateFolder is null)
        {
            return;
        }

        var path = Path.Combine(macro.PrivateFolder, ThumbnailFilename);

        if (!File.Exists(path))
        {
            return;
        }

        image.Save(path);
    }
}