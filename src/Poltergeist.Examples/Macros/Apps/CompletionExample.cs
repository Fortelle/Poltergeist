using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros.Apps;

[ExampleMacro]
public class CompletionExample : BasicMacro
{
    public CompletionExample() : base()
    {
        Title = "Completion";

        Category = "Apps";

        Description = "A macro that demonstrates the use of the CompletionModule.";

        Modules.Add(new CompleteModule());

        OptionDefinitions.Add(new ParameterDefinition<bool>($"throws_exception")
        {
            DisplayLabel = $"Throws exception",
        });

        Execute = (args) =>
        {
            Thread.Sleep(3000);

            if (args.Processor.Options.Get<bool>($"throws_exception"))
            {
                throw new Exception("This is a test exception from the CompletionExample macro.");
            }
        };
    }
}
