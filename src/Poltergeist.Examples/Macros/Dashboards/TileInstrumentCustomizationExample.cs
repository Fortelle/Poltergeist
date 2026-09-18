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
            Icon = new GlyphIcon("\uE709"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = new GlyphIcon("\uE806"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = new GlyphIcon("\uE7E3"),
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
            Icon = new EmojiIcon("✈️"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = new EmojiIcon("🚍"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = new EmojiIcon("⛴️"),
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
            Icon = new TextIcon("A"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = new TextIcon("B"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = new TextIcon("C"),
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
            Icon = new TextIcon("Item 1"),
            Tooltip = $"Custom Item 1",
        });
        instrument.Add(new()
        {
            Icon = new TextIcon("Item 2"),
            Tooltip = $"Custom Item 2",
        });
        instrument.Add(new()
        {
            Icon = new TextIcon("Item 3"),
            Tooltip = $"Custom Item 3",
        });
    }

    private static void CreateCustomTemplateInstrument(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(gi =>
        {
            gi.Title = "Templates:";
            gi.Templates.Add($"success", new() { Color = ThemeColor.Green, Icon = new GlyphIcon("\uE73E") });
            gi.Templates.Add($"failure", new() { Color = ThemeColor.Red, Icon = new GlyphIcon("\uEDAE") });
            gi.Templates.Add($"warning", new() { Color = ThemeColor.Orange, Icon = new GlyphIcon("\uEDB1") });
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
                Icon = new GlyphIcon("\uE734"),
                Tooltip = $"Custom Item {i + 1}",
            });
        }
    }
}
