using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ListInstrumentExample : BasicMacro
{
    public ListInstrumentExample() : base()
    {
        Title = "ListInstrument";

        Category = "Instruments";

        Description = "This example shows how to use the ListInstrument.";

        Execute = (args) =>
        {
            var count = 3;
            var duration = 1000;
            var badIndexes = new int[] { 2, 6 };
            var dashboard = args.Processor.GetService<DashboardService>();

            {
                var instrument = dashboard.Create<ListInstrument>(gi =>
                {
                    gi.Title = "Basic:";
                });

                for (var i = 0; i < count; i++)
                {
                    instrument.Add(new()
                    {
                        Color = ThemeColor.Gray,
                        Icon = IconInfo.FromGlyph("\uE9AE"),
                        Text = $"Step {i + 1}"
                    });
                }

                for (var i = 0; i < count; i++)
                {
                    instrument.Update(i, new()
                    {
                        Color = ThemeColor.Yellow,
                        Icon = IconInfo.FromGlyph("\uF16A")
                    });
                    Thread.Sleep(duration);
                    instrument.Update(i, new()
                    {
                        Color = ThemeColor.Green,
                        Icon = IconInfo.FromGlyph("\uE930")
                    });
                }
            }

            {
                var instrument = dashboard.Create<ListInstrument>(gi =>
                {
                    gi.Title = "Using templates:";
                    gi.Templates.Add("idle", new() { Color = ThemeColor.Gray, Icon = IconInfo.FromGlyph("\uE9AE") });
                    gi.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Icon = IconInfo.FromGlyph("\uF16A") });
                    gi.Templates.Add("success", new() { Color = ThemeColor.Green, Icon = IconInfo.FromGlyph("\uE930") });
                    gi.Templates.Add("failure", new() { Color = ThemeColor.Red, Icon = IconInfo.FromGlyph("\uEA39") });
                });

                for (var i = 0; i < count; i++)
                {
                    instrument.Add(new()
                    {
                        TemplateKey = "idle",
                        Text = $"Step {i + 1}",
                    });
                }

                for (var i = 0; i < count; i++)
                {
                    instrument.Update(i, new()
                    {
                        TemplateKey = "busy"
                    });
                    Thread.Sleep(duration);
                    instrument.Update(i, new()
                    {
                        TemplateKey = "success"
                    });
                }
            }

            {
                var instrument = dashboard.Create<ProgressListInstrument>(gi =>
                {
                    gi.Title = "Using ProgressListInstrument:";
                });

                for (var i = 0; i < count; i++)
                {
                    instrument.Add(new(ProgressStatus.Idle)
                    {
                        Text = $"Step {i + 1}",
                    });
                }

                for (var i = 0; i < count; i++)
                {
                    instrument.Update(i, new(ProgressStatus.Busy));
                    Thread.Sleep(duration);
                    instrument.Update(i, new(ProgressStatus.Success));
                }
            }

            {
                var instrument = dashboard.Create<ProgressListInstrument>(gi =>
                {
                    gi.Title = "ProgressBar:";
                });

                for (var i = 0; i < count; i++)
                {
                    instrument.Add(new(ProgressStatus.Idle)
                    {
                        Text = $"Step {i + 1}",
                    });
                }

                for (var i = 0; i < count; i++)
                {
                    for (var j = 0; j < 10; j++)
                    {
                        var progress = (j + 1) / 10d;
                        instrument.Update(i, new(ProgressStatus.Busy)
                        {
                            Progress = progress,
                            Subtext = $"{progress:P0}"
                        });
                        Thread.Sleep((int)(duration / 5d));
                    }

                    instrument.Update(i, new(ProgressStatus.Success));
                }
            }

        };
    }

}
