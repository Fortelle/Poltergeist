using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.UI.Controls.Options;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ComboBoxOptionControlExample : UnrunnableMacro
{
    public ComboBoxOptionControlExample() : base()
    {
        Title = nameof(ComboBoxOptionControl);

        Category = "Options";

        Description = $"This example defines several options that use the {nameof(ComboBoxOptionControl)}.";

        OptionDefinitions.Add(new ChoiceOption<string>("ChoiceOption<string>", ["text 1", "text 2", "text 3"])
        {
            DisplayLabel = $"ChoiceOption<string>",
            Mode = ChoiceOptionMode.ComboBox,
        });

        OptionDefinitions.Add(new ChoiceOption<int>("ChoiceOption<int>", [100, 200, 300])
        {
            DisplayLabel = $"ChoiceOption<int>",
            Mode = ChoiceOptionMode.ComboBox,
        });

        OptionDefinitions.Add(new OptionDefinition<DayOfWeek>("OptionDefinition<Enum>")
        {
            DisplayLabel = "OptionDefinition<DayOfWeek>",
        });

        OptionDefinitions.Add(new ChoiceOption<DayOfWeek>("ChoiceOption<Enum>", Enum.GetValues<DayOfWeek>())
        {
            DisplayLabel = $"ChoiceOption<DayOfWeek>",
        });

        OptionDefinitions.Add(new IndexChoiceOption<string>("IndexChoiceOption<string>", ["text 1", "text 2", "text 3"])
        {
            DisplayLabel = $"IndexChoiceOption<string>",
            Mode = ChoiceOptionMode.ComboBox,
        });
    }
}
