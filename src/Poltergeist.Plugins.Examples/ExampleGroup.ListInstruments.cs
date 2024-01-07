using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Macros;
using Poltergeist.Common.Structures.Colors;

namespace Poltergeist.Test;

public partial class ExampleGroup
{

    [AutoLoad]
    public BasicMacro ListInstrumentExample = new("example_listinstrument")
    {
        Title = "ListInstrument Example",
        Description = "This example shows how to use the ListInstrument.",

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
                        Glyph = "\uE9AE",
                        Text = $"Step {i + 1}"
                    });
                }

                for (var i = 0; i < count; i++)
                {
                    instrument.Update(i, new()
                    {
                        Color = ThemeColor.Yellow,
                        Glyph = "\uE9F5"
                    });
                    Thread.Sleep(duration);
                    instrument.Update(i, new()
                    {
                        Color = ThemeColor.Green,
                        Glyph = "\uE930"
                    });
                }
            }

            {
                var instrument = dashboard.Create<ListInstrument>(gi =>
                {
                    gi.Title = "Using templates:";
                    gi.Templates.Add("idle", new() { Color = ThemeColor.Gray, Glyph = "\uE9AE" });
                    gi.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Glyph = "\uE9F5" });
                    gi.Templates.Add("success", new() { Color = ThemeColor.Green, Glyph = "\uE930" });
                    gi.Templates.Add("failure", new() { Color = ThemeColor.Red, Glyph = "\uEA39" });
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

        }
    };


    [AutoLoad]
    public BasicMacro ListInstrumentCustomizationExample = new("example_listinstrument_customization")
    {
        Title = "ListInstrument Customization Example",
        Description = "This example shows how to customize the ListInstrument.",
        ShowStatusBar = false,

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();

            {
                var instrument = dashboard.Create<ListInstrument>(li =>
                {
                    li.Title = "Custom Glyph:";
                });

                instrument.Add(new()
                {
                    Glyph = "\uE709",
                    Text = "The quick brown fox jumps over the lazy dog.",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE804",
                    Text = "The quick brown fox jumps over the lazy dog.",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE805",
                    Text = "The quick brown fox jumps over the lazy dog.",
                });
            }

            {
                var instrument = dashboard.Create<ListInstrument>(li =>
                {
                    li.Title = "Custom Emoji:";
                });

                instrument.Add(new()
                {
                    Emoji = "✈️",
                    Text = "The quick brown fox jumps over the lazy dog.",
                });
                instrument.Add(new()
                {
                    Emoji = "🚍",
                    Text = "The quick brown fox jumps over the lazy dog.",
                });
                instrument.Add(new()
                {
                    Emoji = "⛴️",
                    Text = "The quick brown fox jumps over the lazy dog.",
                });
            }

            {
                var instrument = dashboard.Create<ListInstrument>(li =>
                {
                    li.Title = "Templates:";
                    li.Templates.Add($"success", new() { Color = ThemeColor.Green, Glyph = "\uE930" });
                    li.Templates.Add($"failure", new() { Color = ThemeColor.Red, Glyph = "\uEA39" });
                    li.Templates.Add($"warning", new() { Color = ThemeColor.Orange, Glyph = "\uE7BA" });
                });

                instrument.Add(new()
                {
                    TemplateKey = "success",
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Subtext = "success",
                });
                instrument.Add(new()
                {
                    TemplateKey = "failure",
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Subtext = "failure",
                });
                instrument.Add(new()
                {
                    TemplateKey = "warning",
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Subtext = "warning",
                });
            }

            {
                var instrument = dashboard.Create<ListInstrument>(li =>
                {
                    li.Title = "Progress:";
                    li.Templates.Add($"success", new() { Color = ThemeColor.Green, Glyph = "\uE930" });
                    li.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Glyph = "\uE768" });
                });

                instrument.Add(new()
                {
                    TemplateKey = "busy",
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Progress = 0.1,
                    Subtext = "10%",
                });
                instrument.Add(new()
                {
                    TemplateKey = "busy",
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Progress = 0.5,
                    Subtext = "50%",
                });
                instrument.Add(new()
                {
                    TemplateKey = "busy",
                    Text = "The quick brown fox jumps over the lazy dog.",
                    Progress = 1,
                    Subtext = "100%",
                });
            }

            {
                var instrument = dashboard.Create<ListInstrument>(li =>
                {
                    li.Title = "Colors:";
                });

                var values = Enum.GetValues<ThemeColor>();
                for (var i = 0; i < values.Length; i++)
                {
                    instrument.Add(new()
                    {
                        Color = values[i],
                        Glyph = "\uE735",
                        Text = "The quick brown fox jumps over the lazy dog.",
                        Subtext = values[i].ToString(),
                    });
                }
            }

        }

    };



    [AutoLoad]
    public BasicMacro ListInstrumentButtons = new("example_listinstrument_buttons")
    {
        Title = "ListInstrument Button Example",
        Description = "This example shows how to interact with the user through the ListInstrument.",

        UserOptions =
        {
            new OptionItem<bool>("countdown"),
        },

        ExecuteAsync = async (args) =>
        {
            var li = args.Processor.GetService<ProgressListInstrument>();
            li.Title = "Tasks:";

            var instrumentService = args.Processor.GetService<DashboardService>();
            instrumentService.Add(li);

            var loop = true;
            var i = 0;
            var useCountdown = args.Processor.Options.Get<bool>("countdown");

            while (loop)
            {
                for (var j = 0; j < 8; j++)
                {
                    li.Update(i, new(ProgressStatus.Busy)
                    {
                        Progress = (j + 1) / 10d,
                        Text = $"Processing...",
                    });

                    await Task.Delay(100);
                }

                li.Update(i, new(ProgressStatus.Warning)
                {
                    Text = $"Oops! Something went wrong.",
                    Buttons = new[]
                    {
                        new ListInstrumentButton(){
                            Text = "Retry",
                            Callback = () =>
                            {
                                args.Processor.Resume();
                            },
                        },
                        new ListInstrumentButton(){
                            Text = "Continue",
                            CountdownSeconds = useCountdown ? 15 : 0,
                            Callback = () =>
                            {
                                li.Override(i, new(ProgressStatus.Success)
                                {
                                    Text = $"You chose 'continue'.",
                                    Subtext = "Continue",
                                });

                                i++;

                                args.Processor.Resume();
                            },
                        },
                        new ListInstrumentButton(){
                            Text = "Stop",
                            Callback = () =>
                            {
                                li.Override(i, new(ProgressStatus.Failure)
                                {
                                    Text = $"You chose 'Stop'.",
                                    Subtext = "Stop",
                                });

                                loop = false;

                                args.Processor.Resume();
                            },
                        },
                    },
                });

                await args.Processor.Pause();

                if (!loop)
                {
                    break;
                }

                if (i >= 10)
                {
                    break;
                }
            }
        }

    };

}
