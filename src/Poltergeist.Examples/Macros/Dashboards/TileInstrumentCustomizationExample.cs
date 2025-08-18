using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class TileInstrumentCustomizationExample : BasicMacro
{
    public TileInstrumentCustomizationExample() : base()
    {
        Title = "TileInstrument customization";

        Category = "Instruments";

        Description = "This example shows how to customize the TileInstrument.";

        ShowStatusBar = false;

        Execute = (args) =>
        {
            var dashboard = args.Processor.GetService<DashboardService>();

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Custom Glyph:";
                });

                instrument.Add(new()
                {
                    Glyph = "\uE709",
                    Tooltip = $"Custom Item 1",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE806",
                    Tooltip = $"Custom Item 2",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE7E3",
                    Tooltip = $"Custom Item 3",
                });
            }

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Custom Emoji:";
                });

                instrument.Add(new()
                {
                    Emoji = "✈️",
                    Tooltip = $"Custom Item 1",
                });
                instrument.Add(new()
                {
                    Emoji = "🚍",
                    Tooltip = $"Custom Item 2",
                });
                instrument.Add(new()
                {
                    Emoji = "⛴️",
                    Tooltip = $"Custom Item 3",
                });
            }

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Custom Text:";
                });

                instrument.Add(new()
                {
                    Text = "A",
                    Tooltip = $"Custom Item 1",
                });
                instrument.Add(new()
                {
                    Text = "B",
                    Tooltip = $"Custom Item 2",
                });
                instrument.Add(new()
                {
                    Text = "C",
                    Tooltip = $"Custom Item 3",
                });
            }

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Custom Size:";
                    gi.IconWidth = 108;
                    gi.IconHeight = 32;
                });

                instrument.Add(new()
                {
                    Text = "Item 1",
                    Tooltip = $"Custom Item 1",
                });
                instrument.Add(new()
                {
                    Text = "Item 2",
                    Tooltip = $"Custom Item 2",
                });
                instrument.Add(new()
                {
                    Text = "Item 3",
                    Tooltip = $"Custom Item 3",
                });
            }

            {
                var instrument = dashboard.Create<TileInstrument>(gi =>
                {
                    gi.Title = "Templates:";
                    gi.Templates.Add($"success", new() { Color = ThemeColor.Green, Glyph = "\uE73E" });
                    gi.Templates.Add($"failure", new() { Color = ThemeColor.Red, Glyph = "\uEDAE" });
                    gi.Templates.Add($"warning", new() { Color = ThemeColor.Orange, Glyph = "\uEDB1" });
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
                        Glyph = "\uE734",
                        Tooltip = $"Custom Item {i + 1}",
                    });
                }
            }

        };

    }
}

