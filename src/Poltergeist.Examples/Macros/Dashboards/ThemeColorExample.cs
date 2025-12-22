using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ThemeColorExample : BasicMacro
{
    public ThemeColorExample() : base()
    {
        Title = "ThemeColor";

        Category = "Instruments";

        Description = "This example shows various ThemeColor patterns.";

        ShowStatusBar = false;

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();

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

            {
                var instrument = dashboard.Create<IndicatorInstrument>(ii =>
                {
                    ii.Title = "Text:";
                });

                foreach (var color in Enum.GetValues<ThemeColor>())
                {
                    instrument.Add($"{color}", [new("")
                    {
                        Icon = IconInfo.FromGlyph("\uE8D2"),
                        Color = color,
                        Tooltip = $"{color}",
                    }]);
                }
            }

        };

    }

}
