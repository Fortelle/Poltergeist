using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class TileInstrumentCustomizationExample : CommonOneshotMacroBase
{
    public TileInstrumentCustomizationExample() : base()
    {
        Title = "TileInstrument customization";

        Category = "Instruments";

        Description = "This example shows how to customize the TileInstrument.";

        ShowStatusBar = false;
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var dashboard = controller.Processor.GetService<DashboardService>();
        CreateCustomGlyphInstrument(dashboard);
        CreateCustomEmojiInstrument(dashboard);
        CreateCustomTextInstrument(dashboard);
        CreateCustomSizeInstrument(dashboard);
        CreateCustomTemplateInstrument(dashboard);
        CreateCustomColorInstrument(dashboard);
    }

    private static void CreateCustomGlyphInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Custom Glyph:";
        });

        instrument.Add(new()
        {
            Icon = IconInfo.FromGlyph("\uE709"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromGlyph("\uE806"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromGlyph("\uE7E3"),
            Tooltip = $"Custom Item 3",
        });
    }

    private static void CreateCustomEmojiInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Custom Emoji:";
        });

        instrument.Add(new()
        {
            Icon = IconInfo.FromEmoji("✈️"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromEmoji("🚍"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromEmoji("⛴️"),
            Tooltip = $"Custom Item 3",
        });
    }

    private static void CreateCustomTextInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Custom Text:";
        });

        instrument.Add(new()
        {
            Icon = IconInfo.FromText("A"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromText("B"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromText("C"),
            Tooltip = $"Custom Item 3",
        });
    }

    private static void CreateCustomSizeInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Custom Size:";
            gi.IconWidth = 108;
            gi.IconHeight = 32;
        });

        instrument.Add(new()
        {
            Icon = IconInfo.FromText("Item 1"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromText("Item 2"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = IconInfo.FromText("Item 3"),
            Tooltip = $"Custom Item 3",
        });
    }

    private static void CreateCustomTemplateInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Templates:";
            gi.Templates.Add($"success", new() { Color = ThemeColor.Green, Icon = IconInfo.FromGlyph("\uE73E") });
            gi.Templates.Add($"failure", new() { Color = ThemeColor.Red, Icon = IconInfo.FromGlyph("\uEDAE") });
            gi.Templates.Add($"warning", new() { Color = ThemeColor.Orange, Icon = IconInfo.FromGlyph("\uEDB1") });
        });

        instrument.Add(new()
        {
            TemplateKey = "success",
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            TemplateKey = "failure",
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            TemplateKey = "warning",
            Tooltip = $"Custom Item 3",
        });
    }

    private static void CreateCustomColorInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Colors:";
        });

        var values = Enum.GetValues<ThemeColor>();
        for (var i = 0; i < values.Length; i++)
        {
            instrument.Add(new()
            {
                Color = values[i],
                Icon = IconInfo.FromGlyph("\uE734"),
                Tooltip = $"Custom Item {i + 1}",
            });
        }
    }
}
