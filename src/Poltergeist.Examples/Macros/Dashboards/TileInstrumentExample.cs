using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class TileInstrumentExample : CommonOneshotMacroBase
{
    public TileInstrumentExample() : base()
    {
        Title = "TileInstrument";

        Category = "Instruments";

        Description = "This example shows how to use the TileInstrument.";

        ShowStatusBar = false;
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var dashboard = controller.Processor.GetService<DashboardService>();

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
                    Icon = new GlyphIcon("\uF16A")
                    {
                        Animation = new SpinAnimation(),
                    }
                });
                Thread.Sleep(duration);
                instrument.Update(i, new()
                {
                    Color = ThemeColor.Green,
                    Icon = new GlyphIcon("\uE73E")
                });
            }
        }

        {
            var instrument = dashboard.Create<TileInstrument>(gi =>
            {
                gi.Title = "Using templates:";
                gi.Templates.Add("idle", new() { Color = ThemeColor.Gray });
                gi.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Icon = new GlyphIcon("\uF16A") { Animation = new SpinAnimation() } });
                gi.Templates.Add("success", new() { Color = ThemeColor.Green, Icon = new GlyphIcon("\uE73E") });
                gi.Templates.Add("failure", new() { Color = ThemeColor.Red, Icon = new GlyphIcon("\uEDAE") });
                gi.Templates.Add("warning", new() { Color = ThemeColor.Orange, Icon = new GlyphIcon("\uEDB1") });
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
            var instrument = controller.Processor.GetService<DashboardService>().Create<ProgressTileInstrument>(gi =>
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
    }
}
