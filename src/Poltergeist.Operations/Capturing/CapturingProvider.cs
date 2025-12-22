using System.Drawing;
using System.Drawing.Drawing2D;
using Poltergeist.Automations.Components.Hooks;
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

    private readonly HookService HookService;
    private readonly LocatingProvider? LocatingProvider;

    protected CapturingProvider(MacroProcessor processor, LocatingProvider? locatingProvider) : base(processor)
    {
        LocatingProvider = locatingProvider;
        HookService = Processor.GetService<HookService>();

        IsPreviewable = Processor.Options.GetValueOrDefault<bool>(PreviewCaptureKey);
        if (IsPreviewable)
        {
            Instrument = processor.GetService<ImageInstrument>();
            Instrument.Key = PreviewCaptureKey;
            Instrument.Title = "Client:";
            processor.GetService<PanelService>().Create(new("capture_preview_panel", "Capture")
            {
                Instruments =
                {
                    Instrument
                }
            });
            HookService.Register<ClientCapturedHook>(OnClientCaptured);
        }
    }

    public Bitmap Capture(CapturingOptions? options = null)
    {
        Logger.Trace($"Capturing the whole workspace.");
        Logger.IncreaseIndent();

        var ignoresSnapshot = options?.IgnoresSnapshot ?? false;

        Bitmap imageOnWorkspace;

        if (options?.WorkspaceSnapshot is not null && !ignoresSnapshot)
        {
            imageOnWorkspace = new(options.WorkspaceSnapshot);
            Logger.Trace($"Copied an image from {nameof(options)}.{nameof(options.WorkspaceSnapshot)}.");
        }
        else if (options?.SnapshotKey is not null && !ignoresSnapshot)
        {
            var snapshot = GetSnapshot(options.SnapshotKey);
            imageOnWorkspace = new(snapshot);
            Logger.Trace($"Copied an image from the cached snapshot \"{options.SnapshotKey}\".");
        }
        else if (CurrentSnapshotKey is not null && !ignoresSnapshot)
        {
            var snapshot = GetSnapshot(CurrentSnapshotKey);
            imageOnWorkspace = new(snapshot);
            Logger.Trace($"Copied an image from the cached snapshot \"{CurrentSnapshotKey}\".");
        }
        else if (CaptureClientFullHandler is not null)
        {
            var imageOnClient = CaptureClientFullHandler.Invoke();
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = imageOnClient.Size,
                FullImage = imageOnClient,
            });
            imageOnWorkspace = ClientImageToWorkspace(imageOnClient);
        }
        else if (CaptureClientPartHandler is not null && LocatingProvider?.ClientSize is not null)
        {
            var clientSize = LocatingProvider.ClientSize.Value;
            var imageOnClient = CaptureClientPartHandler.Invoke(new Rectangle(default, clientSize));
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = clientSize,
                FullImage = imageOnClient,
            });
            imageOnWorkspace = ClientImageToWorkspace(imageOnClient);
        }
        else if (CaptureClientPartsHandler is not null && LocatingProvider?.ClientSize is not null)
        {
            var clientSize = LocatingProvider.ClientSize.Value;
            var imageOnClient = CaptureClientPartsHandler.Invoke([new Rectangle(default, clientSize)])[0];
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = clientSize,
                FullImage = imageOnClient,
            });
            imageOnWorkspace = ClientImageToWorkspace(imageOnClient);
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} is not implemented correctly.");
        }

        Logger.DecreaseIndent();
        return imageOnWorkspace;
    }

    public Bitmap Capture(Rectangle rectangleOnWorkspace, CapturingOptions? options = null)
    {
        Logger.Trace($"Capturing the part of the workspace.");
        Logger.IncreaseIndent();

        var requiresFullSnapshot = options?.RequiresFullSnapshot ?? false;
        var ignoresSnapshot = options?.IgnoresSnapshot ?? false;

        Bitmap imageOnWorkspace;

        if (options?.WorkspaceSnapshot is not null && !ignoresSnapshot)
        {
            imageOnWorkspace = CropImage(options.WorkspaceSnapshot, rectangleOnWorkspace);
            Logger.Trace($"Cropped an image from {nameof(options)}.{nameof(options.WorkspaceSnapshot)}.", rectangleOnWorkspace);
        }
        else if (options?.SnapshotKey is not null && !ignoresSnapshot)
        {
            var snapshot = GetSnapshot(options.SnapshotKey);
            imageOnWorkspace = CropImage(snapshot, rectangleOnWorkspace);
            Logger.Trace($"Cropped an image from the cached snapshot \"{options.SnapshotKey}\".");
        }
        else if (CurrentSnapshotKey is not null && !ignoresSnapshot)
        {
            var snapshot = GetSnapshot(CurrentSnapshotKey);
            imageOnWorkspace = CropImage(snapshot, rectangleOnWorkspace);
            Logger.Trace($"Cropped an image from the cached snapshot \"{CurrentSnapshotKey}\".");
        }
        else if (CaptureClientPartHandler is not null && !requiresFullSnapshot)
        {
            var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
            var imageOnClient = CaptureClientPartHandler.Invoke(rectangleOnClient);
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = LocatingProvider?.ClientSize,
                ClipImages = [imageOnClient],
                ClipAreas = [rectangleOnClient],
                TargetAreas = [rectangleOnClient],
            });
            imageOnWorkspace = ClientImageToWorkspace(imageOnClient);
        }
        else if (CaptureClientPartsHandler is not null && !requiresFullSnapshot)
        {
            var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
            var imageOnClient = CaptureClientPartsHandler.Invoke([rectangleOnClient])[0];
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = LocatingProvider?.ClientSize,
                ClipImages = [imageOnClient],
                ClipAreas = [rectangleOnClient],
                TargetAreas = [rectangleOnClient],
            });
            imageOnWorkspace = ClientImageToWorkspace(imageOnClient);
        }
        else if (CaptureClientFullHandler is not null)
        {
            using var clientImage = CaptureClientFullHandler.Invoke();
            var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = clientImage.Size,
                FullImage = clientImage,
                TargetAreas = [rectangleOnClient],
            });
            var imageOnClient = CropImage(clientImage, rectangleOnClient);
            imageOnWorkspace = ClientImageToWorkspace(imageOnClient);
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} is not implemented correctly.");
        }

        Logger.DecreaseIndent();
        return imageOnWorkspace;
    }

    public Bitmap[] Capture(Rectangle[] rectanglesOnWorkspace, CapturingOptions? options = null)
    {
        Logger.Trace($"Capturing the parts of the workspace.");
        Logger.IncreaseIndent();

        var requiresFullSnapshot = options?.RequiresFullSnapshot ?? false;
        var ignoresSnapshot = options?.IgnoresSnapshot ?? false;

        Bitmap[] imagesOnWorkspace;

        if (options?.WorkspaceSnapshot is not null && !ignoresSnapshot)
        {
            imagesOnWorkspace = [.. rectanglesOnWorkspace.Select(rect => CropImage(options.WorkspaceSnapshot, rect))];
            Logger.Trace($"Cropped images from {nameof(options)}.{nameof(options.WorkspaceSnapshot)}.", rectanglesOnWorkspace);
        }
        else if (options?.SnapshotKey is not null && !ignoresSnapshot)
        {
            var snapshot = GetSnapshot(options.SnapshotKey);
            imagesOnWorkspace = [.. rectanglesOnWorkspace.Select(rect => CropImage(snapshot, rect))];
            Logger.Trace($"Cropped an image from the cached snapshot \"{options.SnapshotKey}\".");
        }
        else if (CurrentSnapshotKey is not null && !ignoresSnapshot)
        {
            var snapshot = GetSnapshot(CurrentSnapshotKey);
            imagesOnWorkspace = [.. rectanglesOnWorkspace.Select(rect => CropImage(snapshot, rect))];
            Logger.Trace($"Cropped an image from the cached snapshot \"{CurrentSnapshotKey}\".");
        }
        else if (CaptureClientPartsHandler is not null && !requiresFullSnapshot)
        {
            var rectanglesOnClient = rectanglesOnWorkspace.Select(WorkspaceRectangleToClient).ToArray();
            var imagesOnClient = CaptureClientPartsHandler.Invoke(rectanglesOnClient);
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = LocatingProvider?.ClientSize,
                ClipImages = imagesOnClient,
                ClipAreas = rectanglesOnClient,
                TargetAreas = rectanglesOnClient,
            });
            imagesOnWorkspace = [.. imagesOnClient.Select(ClientImageToWorkspace)];
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
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = LocatingProvider?.ClientSize,
                ClipImages = [imageOnClient],
                ClipAreas = [boundsOnClient],
                TargetAreas = rectanglesOnClient,
            });
            var imagesOnClient = rectanglesOnClient.Select(rect => CropImage(imageOnClient, new Rectangle(rect.X - boundsOnClient.X, rect.Y - boundsOnClient.Y, rect.Width, rect.Height))).ToArray();
            imagesOnWorkspace = [.. imagesOnClient.Select(ClientImageToWorkspace)];
        }
        else if (CaptureClientFullHandler is not null)
        {
            var rectanglesOnClient = rectanglesOnWorkspace.Select(WorkspaceRectangleToClient).ToArray();
            using var clientImage = CaptureClientFullHandler.Invoke();
            HookService.Raise<ClientCapturedHook>(new()
            {
                ClientSize = clientImage.Size,
                FullImage = clientImage,
                TargetAreas = rectanglesOnClient,
            });
            var imagesOnClient = rectanglesOnClient.Select(rect => CropImage(clientImage, rect)).ToArray();
            imagesOnWorkspace = [.. imagesOnClient.Select(ClientImageToWorkspace)];
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} is not implemented correctly.");
        }

        Logger.DecreaseIndent();
        return imagesOnWorkspace;
    }

    private Bitmap ClientImageToWorkspace(Bitmap imageOnClient)
    {
        if (LocatingProvider is null)
        {
            return imageOnClient;
        }

        var clientSize = LocatingProvider.ClientSize;
        var workspaceSize = LocatingProvider.WorkspaceSize;

        if (clientSize is null || workspaceSize is null || workspaceSize.Value == clientSize.Value)
        {
            return imageOnClient;
        }

        var sizeOnClient = imageOnClient.Size;
        var sizeOnWorkspace = LocatingProvider.ClientSizeToWorkspace(sizeOnClient);

        var imageOnWorkspace = ResizeImage(imageOnClient, sizeOnWorkspace);

        imageOnClient.Dispose();

        Logger.Trace($"Resized the captured image to match the workspace size: {sizeOnClient} -> {sizeOnWorkspace}.");

        return imageOnWorkspace;
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
        var newBmp = new Bitmap(newSize.Width, newSize.Height);
        using var gra = Graphics.FromImage(newBmp);
        gra.CompositingQuality = CompositingQuality.HighQuality;
        gra.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gra.SmoothingMode = SmoothingMode.HighQuality;
        gra.DrawImage(bmp, 0, 0, newSize.Width, newSize.Height);
        return newBmp;
    }
}
