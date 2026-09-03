using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MinimizationExample : CommonOneshotMacroBase
{
    public MinimizationExample() : base()
    {
        Title = "Minimization";

        Category = "Apps";

        Description = "A macro that minimizes the application window while running.";

        Modules.Add(new MinimizationModule(true));
    }

    protected override void OnExecute(WorkflowController controller)
    {
        Thread.Sleep(3000);
    }
}
