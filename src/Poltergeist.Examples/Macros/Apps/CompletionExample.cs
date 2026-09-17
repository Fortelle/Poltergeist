using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros.Apps;

[ExampleMacro]
public class CompletionExample : CommonOneshotMacroBase
{
    public CompletionExample() : base()
    {
        Title = "Completion";

        Category = "Apps";

        Description = "A macro that demonstrates the use of the CompletionModule.";

        Modules.Add(new CompleteModule());

        OptionDefinitions.Add(new OptionDefinition<bool>($"throws_exception")
        {
            DisplayLabel = $"Throws exception",
        });
    }

    protected override void OnExecute(WorkflowController controller)
    {
        Thread.Sleep(3000);

        if (controller.Processor.Options.Get<bool>($"throws_exception"))
        {
            throw new Exception("This is a test exception from the CompletionExample macro.");
        }
    }
}
