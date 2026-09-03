using Poltergeist.Automations.Macros.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ConfigVariationExample : LoopMacro
{
    public ConfigVariationExample() : base()
    {
        Title = "Config variations";

        Category = "Browsers";

        Description = "This example shows how to define config variations.";

        ConfigVariations.Add(new ConfigVariation()
        {
            Title = "Loop 5 times",
            Description = "Overrides the user options to force the macro to loop 5 times.",
            Icon = "\uE895",
            OptionOverrides =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(LoopPattern.Multiple),
                LoopConfiguralizationModule.CountDefinition.WithValue(5),
            ],
        });

        ExecuteAsync = OnIterateAsync;
    }

    private async Task OnIterateAsync(WorkflowController controller, IterationContext context)
    {
        await Task.Delay(500, controller.Processor.CancellationToken);
    }
};
