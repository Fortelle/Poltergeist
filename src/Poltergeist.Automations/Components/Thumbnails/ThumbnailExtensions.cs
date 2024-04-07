using System.Drawing;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Components.Thumbnails;

public static class ThumbnailExtensions
{
    private const string ThumbnailFilename = "thumbnail.png";

    public static string? GetThumbnailFile(this MacroShell shell)
    {
        if (shell.PrivateFolder is null)
        {
            return null;
        }

        var path = Path.Combine(shell.PrivateFolder, ThumbnailFilename);

        if (!File.Exists(path))
        {
            return null;
        }

        return path;
    }

    public static void SetThumbnailFile(this MacroShell shell, Bitmap image)
    {
        if (shell.PrivateFolder is null)
        {
            return;
        }

        var path = Path.Combine(shell.PrivateFolder, ThumbnailFilename);

        if (!File.Exists(path))
        {
            return;
        }

        image.Save(path);
    }
}