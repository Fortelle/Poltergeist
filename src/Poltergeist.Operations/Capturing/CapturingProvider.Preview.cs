using System.Drawing;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Operations.Capturing;

public partial class CapturingProvider
{
    public const string PreviewCaptureKey = "preview_capture";

    private const int TransparentTileSize = 16;

    private readonly bool IsPreviewable;

    private readonly ImageInstrument? Instrument;

    private readonly Lazy<Brush> TransparentBackgroundBrush = new(() =>
    {
        var tileSize = TransparentTileSize;
        var bmp = new Bitmap(tileSize * 2, tileSize * 2);
        using var graTexture = Graphics.FromImage(bmp);
        graTexture.FillRectangle(Brushes.LightGray, 0, 0, tileSize, tileSize);
        graTexture.FillRectangle(Brushes.LightGray, tileSize, tileSize, tileSize, tileSize);
        return new TextureBrush(bmp);
    });

    private void PushPreview(Bitmap workspaceImage, Rectangle[]? areas = null)
    {
        if (!IsPreviewable)
        {
            return;
        }

        var previewImage = new Bitmap(workspaceImage);

        if (areas?.Length > 0)
        {
            using var gra = Graphics.FromImage(previewImage);
            DrawHighlights(gra, previewImage.Size, areas);
        }

        Instrument!.Update(0, new ImageInstrumentItem(previewImage));
    }

    private void PushPreview(Size? canvasSize, Bitmap[] images, Rectangle[] areas)
    {
        if (!IsPreviewable)
        {
            return;
        }

        if (canvasSize is null)
        {
            return;
        }

        var previewImage = new Bitmap(canvasSize.Value.Width, canvasSize.Value.Height);
        using var gra = Graphics.FromImage(previewImage);

        gra.FillRectangle(TransparentBackgroundBrush.Value, 0, 0, previewImage.Width, previewImage.Height);

        for (var i = 0; i < areas.Length; i++)
        {
            gra.DrawImage(images[i], areas[i]);
        }

        DrawHighlights(gra, previewImage.Size, areas);

        Instrument!.Update(0, new ImageInstrumentItem(previewImage));
    }

    private void PushPreview(Size? canvasSize, Bitmap image, Rectangle imageArea, Rectangle[] areas)
    {
        if (!IsPreviewable)
        {
            return;
        }

        if (canvasSize is null)
        {
            return;
        }

        var previewImage = new Bitmap(canvasSize.Value.Width, canvasSize.Value.Height);
        using var gra = Graphics.FromImage(previewImage);

        gra.FillRectangle(TransparentBackgroundBrush.Value, 0, 0, previewImage.Width, previewImage.Height);

        gra.DrawImage(image, imageArea);

        DrawHighlights(gra, previewImage.Size, [imageArea, ..areas]);

        Instrument!.Update(0, new ImageInstrumentItem(previewImage));
    }

    private static void DrawHighlights(Graphics gra, Size canvasSize, Rectangle[] areas)
    {
        var region = new Region(new Rectangle(0, 0, canvasSize.Width, canvasSize.Height));
        foreach (var area in areas)
        {
            region.Exclude(area);
        }
        using var brush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        gra.FillRegion(brush, region);

        using var font = new Font("Times New Roman", 8);
        using var sf = new StringFormat()
        {
            Alignment = StringAlignment.Far,
        };
        using var pen = new Pen(Color.White)
        {
            DashPattern = [4, 2]
        };
        foreach (var area in areas)
        {
            gra.DrawRectangle(pen, area);
            gra.DrawString($"({area.Left},{area.Top})", font, Brushes.White, new Point(area.Left, area.Top - 16));
            gra.DrawString($"({area.Width}x{area.Height})", font, Brushes.White, new Point(area.Right, area.Bottom), sf);
        }
    }
}
