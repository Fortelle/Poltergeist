using System.Drawing;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Thumbnails;

public static class ThumbnailExtensions
{
    public const string ThumbnailFilename = "thumbnail.png";

    public static void SetThumbnailFile(this IServiceProcessor processor, Bitmap image)
    {
        if (!processor.Environments.TryGetValue<string>("private_folder", out var privateFolder))
        {
            return;
        }

        var path = Path.Combine(privateFolder, ThumbnailFilename);

        if (!File.Exists(path))
        {
            return;
        }

        image.Save(path);
    }
}