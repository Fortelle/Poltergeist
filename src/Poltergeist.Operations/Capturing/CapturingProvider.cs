using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Operations.Capturing;

// todo: support IShape
// warning: croping image before resizing may get an unexpected border. try using CapturingOptions.RequiresFullSnapshot = true to avoid it.
public abstract partial class CapturingProvider : MacroService
{
    protected Func<Bitmap>? CaptureClientFullHandler;
    protected Func<Rectangle, Bitmap>? CaptureClientPartHandler;
    protected Func<Rectangle[], Bitmap[]>? CaptureClientPartsHandler;

    private readonly LocatingProvider? LocatingProvider;

    protected CapturingProvider(MacroProcessor processor, LocatingProvider? locatingProvider) : base(processor)
    {
        LocatingProvider = locatingProvider;

        IsPreviewable = Processor.Options.GetValueOrDefault<bool>(PreviewCaptureKey);
        if (IsPreviewable)
        {
            Instrument = processor.GetService<ImageInstrument>();
            Instrument.Key = PreviewCaptureKey;
            Instrument.Title = "Workspace:";
            processor.GetService<PanelService>().Create(new("capture_preview_panel", "Capture")
            {
                Instruments =
                {
                    Instrument
                }
            });
        }
    }

    public Bitmap Capture(CapturingOptions? options = null)
    {
        Logger.Trace($"Capturing the whole workspace.");
        Logger.IncreaseIndent();

        Bitmap bitmap;

        if (options?.WorkspaceSnapshot is not null)
        {
            bitmap = new(options.WorkspaceSnapshot);
            Logger.Trace($"Copied an image from {nameof(options)}.{nameof(options.WorkspaceSnapshot)}.");
        }
        else if (options?.SnapshotKey is not null)
        {
            var snapshot = GetSnapshot(options.SnapshotKey);
            bitmap = new(snapshot);
            Logger.Trace($"Copied an image from the cached snapshot \"{options.SnapshotKey}\".");
        }
        else if (CurrentSnapshotKey is not null)
        {
            var snapshot = GetSnapshot(CurrentSnapshotKey);
            bitmap = new(snapshot);
            Logger.Trace($"Copied an image from the cached snapshot \"{CurrentSnapshotKey}\".");
        }
        else if (CaptureClientFullHandler is not null)
        {
            bitmap = CaptureClientFullHandler.Invoke();
            PushPreview(bitmap);
            bitmap = ClientImageToWorkspace(bitmap);
        }
        else if (CaptureClientPartHandler is not null && LocatingProvider?.ClientSize is not null)
        {
            var clientSize = LocatingProvider.ClientSize.Value;
            bitmap = CaptureClientPartHandler.Invoke(new Rectangle(default, clientSize));
            PushPreview(bitmap);
            bitmap = ClientImageToWorkspace(bitmap);
        }
        else if (CaptureClientPartsHandler is not null && LocatingProvider?.ClientSize is not null)
        {
            var clientSize = LocatingProvider.ClientSize.Value;
            bitmap = CaptureClientPartsHandler.Invoke([new Rectangle(default, clientSize)])[0];
            PushPreview(bitmap);
            bitmap = ClientImageToWorkspace(bitmap);
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} is not implemented correctly.");
        }

        Logger.DecreaseIndent();
        return bitmap;
    }

    public Bitmap Capture(Rectangle rectangleOnWorkspace, CapturingOptions? options = null)
    {
        Logger.Trace($"Capturing the part of the workspace.");
        Logger.IncreaseIndent();

        var requiresFullSnapshot = options?.RequiresFullSnapshot ?? false;

        Bitmap bitmap;

        if (options?.WorkspaceSnapshot is not null)
        {
            bitmap = CropImage(options.WorkspaceSnapshot, rectangleOnWorkspace);
            Logger.Trace($"Cropped an image from {nameof(options)}.{nameof(options.WorkspaceSnapshot)}.", rectangleOnWorkspace);
        }
        else if (options?.SnapshotKey is not null)
        {
            var snapshot = GetSnapshot(options.SnapshotKey);
            bitmap = CropImage(snapshot, rectangleOnWorkspace);
            Logger.Trace($"Cropped an image from the cached snapshot \"{options.SnapshotKey}\".");
        }
        else if (CurrentSnapshotKey is not null)
        {
            var snapshot = GetSnapshot(CurrentSnapshotKey);
            bitmap = CropImage(snapshot, rectangleOnWorkspace);
            Logger.Trace($"Cropped an image from the cached snapshot \"{CurrentSnapshotKey}\".");
        }
        else if (CaptureClientPartHandler is not null && !requiresFullSnapshot)
        {
            var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
            bitmap = CaptureClientPartHandler.Invoke(rectangleOnClient);
            PushPreview(LocatingProvider?.ClientSize, [bitmap], [rectangleOnClient]);
            bitmap = ClientImageToWorkspace(bitmap);
        }
        else if (CaptureClientPartsHandler is not null && !requiresFullSnapshot)
        {
            var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
            bitmap = CaptureClientPartsHandler.Invoke([rectangleOnClient])[0];
            PushPreview(bitmap, [rectangleOnClient]);
            bitmap = ClientImageToWorkspace(bitmap);
        }
        else if (CaptureClientFullHandler is not null)
        {
            using var clientImage = CaptureClientFullHandler.Invoke();
            var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
            PushPreview(clientImage, [rectangleOnClient]);
            var imageOnClient = CropImage(clientImage, rectangleOnClient);
            bitmap = ClientImageToWorkspace(imageOnClient);
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} is not implemented correctly.");
        }

        Logger.DecreaseIndent();
        return bitmap;
    }

    public Bitmap[] Capture(Rectangle[] rectanglesOnWorkspace, CapturingOptions? options = null)
    {
        Logger.Trace($"Capturing the parts of the workspace.");
        Logger.IncreaseIndent();

        var requiresFullSnapshot = options?.RequiresFullSnapshot ?? false;

        Bitmap[] bitmaps;

        if (options?.WorkspaceSnapshot is not null)
        {
            bitmaps = [.. rectanglesOnWorkspace.Select(rect => CropImage(options.WorkspaceSnapshot, rect))];
            Logger.Trace($"Cropped images from {nameof(options)}.{nameof(options.WorkspaceSnapshot)}.", rectanglesOnWorkspace);
        }
        else if (options?.SnapshotKey is not null)
        {
            var snapshot = GetSnapshot(options.SnapshotKey);
            bitmaps = [.. rectanglesOnWorkspace.Select(rect => CropImage(snapshot, rect))];
            Logger.Trace($"Cropped an image from the cached snapshot \"{options.SnapshotKey}\".");
        }
        else if (CurrentSnapshotKey is not null)
        {
            var snapshot = GetSnapshot(CurrentSnapshotKey);
            bitmaps = [.. rectanglesOnWorkspace.Select(rect => CropImage(snapshot, rect))];
            Logger.Trace($"Cropped an image from the cached snapshot \"{CurrentSnapshotKey}\".");
        }
        else if (CaptureClientPartsHandler is not null && !requiresFullSnapshot)
        {
            var rectanglesOnClient = rectanglesOnWorkspace.Select(WorkspaceRectangleToClient).ToArray();
            var imagesOnClient = CaptureClientPartsHandler.Invoke(rectanglesOnClient);
            PushPreview(LocatingProvider?.ClientSize, imagesOnClient, rectanglesOnClient);
            bitmaps = [.. imagesOnClient.Select(ClientImageToWorkspace)];
        }
        else if (CaptureClientPartHandler is not null && !requiresFullSnapshot)
        {
            var rectanglesOnClient = rectanglesOnWorkspace.Select(WorkspaceRectangleToClient).ToArray();
            var l = rectanglesOnClient.Min(x => x.X);
            var t = rectanglesOnClient.Min(x => x.Y);
            var r = rectanglesOnClient.Max(x => x.Right);
            var b = rectanglesOnClient.Max(x => x.Bottom);
            var boundsOnClient = Rectangle.FromLTRB(l, t, r, b);
            var imageOnClient = CaptureClientPartHandler.Invoke(boundsOnClient);
            var imagesOnClient = rectanglesOnClient.Select(rect => CropImage(imageOnClient, new Rectangle(rect.X - boundsOnClient.X, rect.Y - boundsOnClient.Y, rect.Width, rect.Height))).ToArray();
            PushPreview(LocatingProvider?.ClientSize, imageOnClient, boundsOnClient, rectanglesOnClient);
            bitmaps = [.. imagesOnClient.Select(ClientImageToWorkspace)];
        }
        else if (CaptureClientFullHandler is not null)
        {
            var rectanglesOnClient = rectanglesOnWorkspace.Select(WorkspaceRectangleToClient).ToArray();
            var clientImage = CaptureClientFullHandler.Invoke();
            var imagesOnClient = rectanglesOnClient.Select(rect => CropImage(clientImage, rect)).ToArray();
            PushPreview(clientImage, rectanglesOnClient);
            bitmaps = [.. imagesOnClient.Select(ClientImageToWorkspace)];
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} is not implemented correctly.");
        }

        Logger.DecreaseIndent();
        return bitmaps;
    }

    private Bitmap ClientImageToWorkspace(Bitmap bmp)
    {
        if (LocatingProvider is null)
        {
            return bmp;
        }

        var clientSize = LocatingProvider.ClientSize;
        var workspaceSize = LocatingProvider.WorkspaceSize;

        if (clientSize is null || workspaceSize is null || workspaceSize.Value == clientSize.Value)
        {
            return bmp;
        }

        Logger.TraceImage(bmp);

        var sizeOnClient = bmp.Size;
        var sizeOnWorkspace = LocatingProvider.ClientSizeToWorkspace(sizeOnClient);

        var newBmp = ResizeImage(bmp, sizeOnWorkspace);

        bmp.Dispose();

        Logger.Trace($"Resized the captured image to match the workspace size: {sizeOnClient} -> {sizeOnWorkspace}.");

        return newBmp;
    }

    private Rectangle WorkspaceRectangleToClient(Rectangle rectangleOnWorkspace)
    {
        if (LocatingProvider is null)
        {
            return rectangleOnWorkspace;
        }
        else
        {
            return LocatingProvider.WorkspaceRectangleToClient(rectangleOnWorkspace);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ReleaseAllSnapshots();

            if (TransparentBackgroundBrush.IsValueCreated)
            {
                TransparentBackgroundBrush.Value.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    protected static Bitmap CropImage(Bitmap bmpSrc, Rectangle rectSrc)
    {
        var bmp = new Bitmap(rectSrc.Width, rectSrc.Height);
        using var gra = Graphics.FromImage(bmp);
        gra.DrawImage(bmpSrc, new Rectangle(0, 0, rectSrc.Width, rectSrc.Height), rectSrc, GraphicsUnit.Pixel);
        return bmp;
    }

    protected static Bitmap ResizeImage(Bitmap bmp, Size newSize)
    {
        var newBmp = new Bitmap(bmp, newSize.Width, newSize.Height);
        using var gra = Graphics.FromImage(newBmp);
        gra.CompositingQuality = CompositingQuality.HighQuality;
        gra.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gra.SmoothingMode = SmoothingMode.HighQuality;
        gra.DrawImage(bmp, 0, 0, newSize.Width, newSize.Height);
        return newBmp;
    }
}
