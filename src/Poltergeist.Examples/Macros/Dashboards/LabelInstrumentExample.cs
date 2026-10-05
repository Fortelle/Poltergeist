using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class LabelInstrumentExample : CommonOneshotMacroBase
{
    public LabelInstrumentExample() : base()
    {
        Title = "LabelInstrument";

        Category = "Instruments";

        Description = "This example shows how to use the LabelInstrument.";

        ShowStatusBar = false;
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var dashboard = controller.Processor.GetService<DashboardService>();

        {
            var instrument = dashboard.Create<LabelInstrument>(li =>
            {
                li.Title = "Basic:";
                li.MaximumColumns = 4;
                li.HeaderPosition = LabelHeaderPosition.TopLeft;
                li.TextPosition = LabelTextPosition.Right;
            });

            instrument.Add(new()
            {
                Color = ThemeColor.Red,
                Header = "Year",
                Text = DateTime.Now.Year.ToString(),
                HeaderIcon = new GlyphIcon("\uE787"),
            });

            instrument.Add(new()
            {
                Color = ThemeColor.Green,
                Header = "Battery",
                Text = "100",
                Subtext = "%",
                HeaderIcon = new GlyphIcon("\uEBA0"),
            });

            instrument.Add(new()
            {
                Header = "Header",
                Text = "Text",
                Subtext = "Subtext",
            });
        }

        {
            var instrument = dashboard.Create<LabelInstrument>(li =>
            {
                li.Title = "Icons:";
                li.MaximumColumns = 4;
                li.HeaderPosition = LabelHeaderPosition.Hidden;
                li.TextPosition = LabelTextPosition.Center;
            });

            instrument.Add(new()
            {
                Text = "Left",
                LeftIcon = new GlyphIcon("\uE8E1"),
            });

            instrument.Add(new()
            {
                Text = "Right",
                RightIcon = new GlyphIcon("\uE8E0"),
            });

            instrument.Add(new()
            {
                Text = "Both",
                LeftIcon = new GlyphIcon("\uE8E1"),
                RightIcon = new GlyphIcon("\uE8E0"),
            });
        }

        {
            var instrument = dashboard.Create<LabelInstrument>(li =>
            {
                li.Title = "Side text:";
                li.MaximumColumns = 4;
                li.HeaderPosition = LabelHeaderPosition.Hidden;
                li.TextPosition = LabelTextPosition.Center;
            });

            instrument.Add(new()
            {
                Text = "Text",
                LeftText = "Left",
            });

            instrument.Add(new()
            {
                Text = "Text",
                RightText = "Right",
            });

            instrument.Add(new()
            {
                Text = "Both",
                LeftText = "Left",
                RightText = "Right",
            });
        }

        {
            dashboard.Create<LabelInstrument>(li =>
            {
                li.Title = nameof(LabelHeaderPosition) + ":";
            });
            foreach (var style in Enum.GetValues<LabelHeaderPosition>())
            {
                var instrument = dashboard.Create<LabelInstrument>(li =>
                {
                    li.HeaderPosition = style;
                    li.TextPosition = LabelTextPosition.Center;
                    li.MaximumColumns = 4;
                });
                instrument.Add(new()
                {
                    Color = ThemeColor.Blue,
                    Header = style.ToString(),
                    Text = "Text",
                    HeaderIcon = new GlyphIcon("\uE787"),
                });
            }
        }
    }
}
