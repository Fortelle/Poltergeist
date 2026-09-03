using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
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

        var instrument = dashboard.Create<LabelInstrument>(gi =>
        {
            gi.Title = "Basic:";
            gi.MaximumColumns = 4;
        });

        instrument.Add(new()
        {
            Color = ThemeColor.Red,
            Label = "Year",
            Text = DateTime.Now.Year.ToString(),
            Icon = new("\uE787"),
        });

        instrument.Add(new()
        {
            Color = ThemeColor.Green,
            Label = "Month",
            Text = DateTime.Now.Month.ToString(),
        });

        instrument.Add(new()
        {
            Label = "Day",
            Text = DateTime.Now.Day.ToString(),
        });
    }
}
