using System.Drawing;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Operations.Capturing;

public class CapturingPreviewModule : MacroModule
{
    public static readonly OptionDefinition<bool> PreviewCaptureOption = new("preview_capture")
    {
        DisplayLabel = "Preview captured image",
        Category = "Debug",
        Status = ParameterStatus.DevelopmentOnly,
    };

    private const int TransparentTileSize = 16;

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

        macro.OptionDefinitions.Add(PreviewCaptureOption);
    }

    public override bool Validate(IMacroProcessorInformation processor)
    {
        return processor.Options.GetValueOrDefault(PreviewCaptureOption);
    }

    [MacroHook]
    public static void OnClientCaptured(IMacroProcessorShared processor, ClientCapturedHook hook)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(hook.ClipImages?.Length, hook.ClipAreas?.Length);

        var clientSize = hook.ClientSize ?? hook.FullImage?.Size;

        if (clientSize is null)
        {
            return;
        }

        var previewImage = new Bitmap(clientSize.Value.Width, clientSize.Value.Height);

        using var gra = Graphics.FromImage(previewImage);

        if (hook.FullImage is not null)
        {
            gra.DrawImage(hook.FullImage, 0, 0);
        }
        else
        {
            var brush = processor.SessionStorage.GetOrAdd("transparent_background_brush", static () =>
            {
                var tileSize = TransparentTileSize;
                var bmp = new Bitmap(tileSize * 2, tileSize * 2);
                using var graTexture = Graphics.FromImage(bmp);
                graTexture.FillRectangle(Brushes.LightGray, 0, 0, tileSize, tileSize);
                graTexture.FillRectangle(Brushes.LightGray, tileSize, tileSize, tileSize, tileSize);
                return new TextureBrush(bmp);
            });
            gra.FillRectangle(brush, 0, 0, previewImage.Width, previewImage.Height);
        }

        if (hook.ClipImages?.Length > 0 && hook.ClipImages.Length == hook.ClipAreas?.Length)
        {
            for (var i = 0; i < hook.ClipImages.Length; i++)
            {
                gra.DrawImage(hook.ClipImages[i], hook.ClipAreas[i]);
            }
        }

        if (hook.TargetAreas?.Length > 0)
        {
            DrawHighlights(gra, clientSize.Value, hook.TargetAreas);
        }

        var instrument = processor.SessionStorage.GetOrAdd("capturing_preview_instrument", () =>
        {
            var instrument = processor.GetService<ImageInstrument>();
            instrument.Key = "capture_preview";
            instrument.Title = "Client:";
            processor.GetService<PanelService>().Create(new("capture_preview_panel", "Capture")
            {
                Instruments =
                {
                    instrument
                }
            });
            return instrument;
        });

        instrument.Update(0, new ImageInstrumentItem(previewImage));
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
