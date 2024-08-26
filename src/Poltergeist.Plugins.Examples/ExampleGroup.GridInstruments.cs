using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Test;

public partial class ExampleGroup
{

    [AutoLoad]
    public BasicMacro GridInstrumentExample = new("example_gridinstrument")
    {
        Title = "GridInstrument Example",
        Description = "This example shows how to use the GridInstrument.",

        ShowStatusBar = false,
        IsSingleton = true,

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();

            var updateCount = 5;
            var duration = 500;

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Basic:";
                    gi.AddPlaceholders(updateCount, new() { Color = ThemeColor.Gray });
                });

                for (var i = 0; i < updateCount; i++)
                {
                    instrument.Update(i, new() {
                        Color = ThemeColor.Yellow,
                        Glyph = "\uE9F5"
                    });
                    Thread.Sleep(duration);
                    instrument.Update(i, new() {
                        Color = ThemeColor.Green,
                        Glyph = "\uE73E"
                    });
                }
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Using templates:";
                    gi.Templates.Add("idle", new() { Color = ThemeColor.Gray, Glyph = null });
                    gi.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Glyph = "\uE9F5" });
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
                var instrument = args.Processor.GetService<DashboardService>().Create<ProgressGridInstrument>(gi =>
                {
                    gi.Title = "Using ProgressGridInstrument:";
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

    };

    [AutoLoad]
    public BasicMacro GridInstrumentCustomization = new("example_gridinstrument_customization")
    {
        Title = "GridInstrument Customization Example",
        Description = "This example shows how to customize the GridInstrument.",

        ShowStatusBar = false,
        IsSingleton = true,

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Custom Glyph:";
                });

                instrument.Add(new()
                {
                    Glyph = "\uE709",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE806",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE7E3",
                });
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Custom Emoji:";
                });

                instrument.Add(new()
                {
                    Emoji = "✈️",
                });
                instrument.Add(new()
                {
                    Emoji = "🚍",
                });
                instrument.Add(new()
                {
                    Emoji = "⛴️",
                });
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Custom Text:";
                });

                instrument.Add(new()
                {
                    Text = "A",
                });
                instrument.Add(new()
                {
                    Text = "B",
                });
                instrument.Add(new()
                {
                    Text = "C",
                });
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Custom Size:";
                    gi.IconWidth = 108;
                    gi.IconHeight = 32;
                });

                instrument.Add(new()
                {
                    Text = "Item 1",
                });
                instrument.Add(new()
                {
                    Text = "Item 2",
                });
                instrument.Add(new()
                {
                    Text = "Item 3",
                });
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Template:";
                    gi.Templates.Add($"success", new() { Color = ThemeColor.Green, Glyph = "\uE73E" });
                    gi.Templates.Add($"failure", new() { Color = ThemeColor.Red, Glyph = "\uEDAE" });
                    gi.Templates.Add($"warning", new() { Color = ThemeColor.Orange, Glyph = "\uEDB1" });
                });

                instrument.Add(new()
                {
                    TemplateKey = "success",
                });
                instrument.Add(new()
                {
                    TemplateKey = "failure",
                });
                instrument.Add(new()
                {
                    TemplateKey = "warning",
                });
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Colors:";
                });

                var values = Enum.GetValues<ThemeColor>();
                for (var i = 0; i < values.Length; i++)
                {
                    instrument.Add(new()
                    {
                        Color = values[i],
                    });
                }
            }

            {
                var instrument = dashboard.Create<GridInstrument>(gi =>
                {
                    gi.Title = "Max Columns of 10:";
                    gi.MaximumColumns = 10;
                });

                for (var i = 0; i < 25; i++)
                {
                    instrument.Add(new());
                }
            }


        }

    };

}
