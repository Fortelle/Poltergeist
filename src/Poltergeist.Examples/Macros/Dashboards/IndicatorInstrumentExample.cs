using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class IndicatorInstrumentExample : BasicMacro
{
    public IndicatorInstrumentExample() : base()
    {
        Title = "IndicatorInstrument";

        Category = "Instruments";

        Description = "This example shows how to use the IndicatorInstrument.";

        ShowStatusBar = false;

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();


            {
                var instrument = dashboard.Create<IndicatorInstrument>(x =>
                {
                    x.Title = "No border:";
                });

                foreach (var color in Enum.GetValues<ThemeColor>())
                {
                    instrument.Add($"{color}", [new("")
                    {
                        Icon = new("\uE945"),
                        Color = color,
                        Tooltip = $"{color}",
                    }]);
                }
            }

            {
                var instrument = dashboard.Create<IndicatorInstrument>(x =>
                {
                    x.Title = "Bordered:";
                });

                foreach (var color in Enum.GetValues<ThemeColor>())
                {
                    instrument.Add($"{color}", [new("")
                    {
                        Icon = new("\uE945"),
                        Color = color,
                        Tooltip = $"{color}",
                        Bordered = true,
                    }]);
                }
            }

            {
                var instrument = dashboard.Create<IndicatorInstrument>(x =>
                {
                    x.Title = "Filled:";
                });

                foreach (var color in Enum.GetValues<ThemeColor>())
                {
                    instrument.Add($"{color}", [new("")
                    {
                        Icon = new("\uE945"),
                        Color = color,
                        Tooltip = $"{color}",
                        Filled = true,
                    }]);
                }
            }

            {
                var instrument = dashboard.Create<IndicatorInstrument>(x =>
                {
                    x.Title = "Breathing:";
                });

                instrument.Add($"", [new("")
                {
                    Icon = new("\uE95E"),
                    Color = ThemeColor.Red,
                    Motion = IndicatorMotion.Breathing,
                }]);
            }

            {
                var instrument = dashboard.Create<IndicatorInstrument>(x =>
                {
                    x.Title = "Blinking:";
                });

                instrument.Add($"", [new("")
                {
                    Icon = new("\uE7BA"),
                    Color = ThemeColor.Red,
                    Motion = IndicatorMotion.Blinking,
                }]);
            }

            {
                var instrument = dashboard.Create<IndicatorInstrument>(x =>
                {
                    x.Title = "Switching:";
                });

                instrument.Add("item1", [
                    new("pattern1")
                    {
                        Color = ThemeColor.Spring,
                        Icon = new("\uEBAA")
                    },
                    new("pattern2")
                    {
                        Color = ThemeColor.Spring,
                        Icon = new("\uEBA9")
                    },
                    new("pattern3")
                    {
                        Color = ThemeColor.Green,
                        Icon = new("\uEBA8")
                    },
                    new("pattern4")
                    {
                        Color = ThemeColor.Green,
                        Icon = new("\uEBA7")
                    },
                    new("pattern5")
                    {
                        Color = ThemeColor.Chartreuse,
                        Icon = new("\uEBA6")
                    },
                    new("pattern6")
                    {
                        Color = ThemeColor.Chartreuse,
                        Icon = new("\uEBA5")
                    },
                    new("pattern7")
                    {
                        Color = ThemeColor.Yellow,
                        Icon = new("\uEBA4")
                    },
                    new("pattern8")
                    {
                        Color = ThemeColor.Yellow,
                        Icon = new("\uEBA3")
                    },
                    new("pattern9")
                    {
                        Color = ThemeColor.Orange,
                        Icon = new("\uEBA2")
                    },
                    new("pattern10")
                    {
                        Color = ThemeColor.Orange,
                        Icon = new("\uEBA1")
                    },
                    new("pattern11")
                    {
                        Color = ThemeColor.Red,
                        Icon = new("\uEBA0"),
                        Motion = IndicatorMotion.Blinking
                    },
                    ]);

                for (var i = 2; i <= 11; i++)
                {
                    Thread.Sleep(1000);
                    instrument.Switch("item1", $"pattern{i}");
                }
            }
        };
    }
}
