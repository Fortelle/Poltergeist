using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class TileInstrumentExample : BasicMacro
{
    public TileInstrumentExample() : base()
    {
        Title = "TileInstrument";

        Category = "Instruments";

        Description = "This example shows how to use the TileInstrument.";

        ShowStatusBar = false;

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();

            var updateCount = 5;
            var duration = 500;

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Basic:";
                    gi.AddPlaceholders(updateCount, new() { Color = ThemeColor.Gray });
                });

                for (var i = 0; i < updateCount; i++)
                {
                    instrument.Update(i, new()
                    {
                        Color = ThemeColor.Yellow,
                        Glyph = "\uF16A"
                    });
                    Thread.Sleep(duration);
                    instrument.Update(i, new()
                    {
                        Color = ThemeColor.Green,
                        Glyph = "\uE73E"
                    });
                }
            }

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Using templates:";
                    gi.Templates.Add("idle", new() { Color = ThemeColor.Gray, Glyph = null });
                    gi.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Glyph = "\uF16A" });
                    gi.Templates.Add("success", new() { Color = ThemeColor.Green, Glyph = "\uE73E" });
                    gi.Templates.Add("failure", new() { Color = ThemeColor.Red, Glyph = "\uEDAE" });
                    gi.Templates.Add("warning", new() { Color = ThemeColor.Orange, Glyph = "\uEDB1" });
                    gi.AddPlaceholders(updateCount, new("idle"));
                });

                for (var i = 0; i < updateCount; i++)
                {
                    instrument.Update(i, new("busy"));
                    Thread.Sleep(duration);
                    instrument.Update(i, new("success"));
                }
            }

            {
                var instrument = args.Processor.GetService<DashboardService>().Create<ProgressTileInstrument>(gi =>
                {
                    gi.Title = "Using ProgressTileInstrument:";
                    gi.AddPlaceholders(updateCount, new(ProgressStatus.Idle));
                });

                for (var i = 0; i < updateCount; i++)
                {
                    instrument.Update(i, new(ProgressStatus.Busy));
                    Thread.Sleep(duration);
                    instrument.Update(i, new(ProgressStatus.Success));
                }
            }

        };

    }

}
