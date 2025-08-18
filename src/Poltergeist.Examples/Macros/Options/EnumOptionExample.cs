using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class EnumOptionExample : MacroBase
{
    public EnumOptionExample() : base()
    {
        Title = "Enum options";

        Category = "Options";

        Description = "This example defines several options of enum types.";

        OptionDefinitions.Add(new OptionDefinition<DayOfWeek>("OptionDefinition")
        {
            DisplayLabel = "OptionDefinition<DayOfWeek>",
        });

        OptionDefinitions.Add(new EnumOption<DayOfWeek>("EnumOption")
        {
            DisplayLabel = "EnumOption<DayOfWeek>",
            Description = "Alias for OptionDefinition<DayOfWeek>",
        });

        OptionDefinitions.Add(new ChoiceOption<DayOfWeek>("ChoiceOption", [DayOfWeek.Sunday, DayOfWeek.Saturday])
        {
            DisplayLabel = $"ChoiceOption<DayOfWeek>",
            Description = $"[DayOfWeek.Sunday, DayOfWeek.Saturday]",
        });
    }
}
