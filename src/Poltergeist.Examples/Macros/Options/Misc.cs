using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class Misc : UnrunnableMacro
{
    public Misc() : base()
    {
        Title = "Misc";

        Category = "Options";

        OptionDefinitions.Add(new OptionDefinition<TimeOnly>("TimeOnly")
        {
            DisplayLabel = "OptionDefinition<TimeOnly>",
        });
        OptionDefinitions.Add(new OptionDefinition<HotKey>("HotKey")
        {
            DisplayLabel = "OptionDefinition<HotKey>",
        });
    }
}
