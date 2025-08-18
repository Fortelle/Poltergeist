using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ListInstrumentCustomizationExample : BasicMacro
{
    public ListInstrumentCustomizationExample() : base()
    {
        Title = "ListInstrument customization";

        Category = "Instruments";

        Description = "This example shows how to customize the ListInstrument.";

        ShowStatusBar = false;

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
                    Text = "Custom Item 1",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE804",
                    Text = "Custom Item 2",
                });
                instrument.Add(new()
                {
                    Glyph = "\uE7E3",
                    Text = "Custom Item 3",
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
                    Text = "Custom Item 1",
                });
                instrument.Add(new()
                {
                    Emoji = "🚍",
                    Text = "Custom Item 2",
                });
                instrument.Add(new()
                {
                    Emoji = "⛴️",
                    Text = "Custom Item 3",
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
                    Text = "Custom Item 1",
                    Subtext = "success",
                });
                instrument.Add(new()
                {
                    TemplateKey = "failure",
                    Text = "Custom Item 2",
                    Subtext = "failure",
                });
                instrument.Add(new()
                {
                    TemplateKey = "warning",
                    Text = "Custom Item 3",
                    Subtext = "warning",
                });
            }

            {
                var instrument = dashboard.Create<ListInstrument>(li =>
                {
                    li.Title = "Progress:";
                    li.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Glyph = "\uEA3A" });
                });

                instrument.Add(new()
                {
                    TemplateKey = "busy",
                    Text = "Custom Item 1",
                    Progress = 0.1,
                    Subtext = "10%",
                });
                instrument.Add(new()
                {
                    TemplateKey = "busy",
                    Text = "Custom Item 2",
                    Progress = 0.5,
                    Subtext = "50%",
                });
                instrument.Add(new()
                {
                    TemplateKey = "busy",
                    Text = "Custom Item 3",
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
                        Glyph = "\uE734",
                        Text = $"Custom Item {i + 1}",
                        Subtext = $"{values[i]}",
                    });
                }
            }

        };

        }
    }
