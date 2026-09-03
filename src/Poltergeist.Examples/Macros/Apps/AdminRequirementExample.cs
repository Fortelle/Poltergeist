using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class AdminRequirementExample : CommonOneshotMacroBase
{
    public AdminRequirementExample() : base()
    {
        Title = "Admin Requirement";

        Category = "Apps";

        Description = "A macro that requires administrator privileges to run.";

        RequiresAdmin = true;
    }

    protected override Task OnExecuteAsync(WorkflowController controller)
    {
        controller.Outputer.Write($"Hello world!");

        return Task.CompletedTask;
    }
}
