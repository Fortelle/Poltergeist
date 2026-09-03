using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ThemeColorExample : CommonOneshotMacroBase
{
    public ThemeColorExample() : base()
    {
        Title = "ThemeColor";

        Category = "Instruments";

        Description = "This example shows various ThemeColor patterns.";

        ShowStatusBar = false;
    }


    protected override void OnExecute(WorkflowController controller)
    {
        var dashboard = controller.Processor.GetService<DashboardService>();

        CreateTiles(dashboard);
        CreateTexts(dashboard);
    }

    private static void CreateTiles(DashboardService dashboard)
    {
        var instrument = dashboard.Create<TileInstrument>(ti =>
        {
            ti.Title = "Tiles:";
        });

        foreach (var color in Enum.GetValues<ThemeColor>())
        {
            instrument.Add(new()
            {
                Icon = IconInfo.FromGlyph("\uE8D2"),
                Color = color,
                Tooltip = $"{color}",
            });
        }
    }

    private static void CreateTexts(DashboardService dashboard)
    {
        var instrument = dashboard.Create<IndicatorInstrument>(ii =>
        {
            ii.Title = "Text:";
        });

        foreach (var color in Enum.GetValues<ThemeColor>())
        {
            instrument.Add($"{color}", [
                new("")
                {
                    Icon = IconInfo.FromGlyph("\uE8D2"),
                    Color = color,
                    Tooltip = $"{color}",
                }
                ]);
        }
    }

}
