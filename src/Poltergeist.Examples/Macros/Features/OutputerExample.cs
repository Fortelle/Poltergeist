using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class OutputerExample : CommonOneshotMacroBase
{
    public OutputerExample() : base()
    {
        Title = nameof(OutputService);

        Category = "Features";

        Description = "This example shows how to print messages to the dashboard by using ArgumentService.Outputer.";
    }

    protected override void OnExecute(WorkflowController controller)
    {
        for (var i = 0; i < 3; i++)
        {
            controller.Outputer.NewGroup($"Group {i + 1}:");
            for (var j = 0; j < 3; j++)
            {
                Thread.Sleep(100);
                controller.Outputer.Write($"Item {j + 1}");
            }
        }
    }
}
