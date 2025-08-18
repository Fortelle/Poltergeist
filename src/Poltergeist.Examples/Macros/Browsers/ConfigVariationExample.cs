using Poltergeist.Automations.Components.Loops;
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
            OptionOverrides = new()
            {
                { LoopService.ConfigEnableKey, true },
                { LoopService.ConfigCountKey, 5 },
            },
        });

        Iterate = (args) =>
        {
            Thread.Sleep(500);
        };
    }
};
