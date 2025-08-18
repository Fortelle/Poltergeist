using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ChoiceOptionExample : UnrunnableMacro
{
    public ChoiceOptionExample() : base()
    {
        Title = "Choice options";

        Category = "Options";

        Description = "This example defines several choice options of different types.";

        OptionDefinitions.Add(new ChoiceOption<string>("ChoiceOption<string>", ["Item1", "Item2", "Item3"])
        {
            DisplayLabel = $"ChoiceOption<string>",
            Mode = ChoiceOptionMode.ComboBox,
        });

        OptionDefinitions.Add(new ChoiceOption<int>("ChoiceOption<int>", [100, 200, 300])
        {
            DisplayLabel = $"ChoiceOption<int>",
            Mode = ChoiceOptionMode.ComboBox,
        });

        OptionDefinitions.Add(new ChoiceOption<DayOfWeek>("ChoiceOption<EnumType>", Enum.GetValues<DayOfWeek>())
        {
            DisplayLabel = $"ChoiceOption<EnumType>",
        });

        OptionDefinitions.Add(new ChoiceOption<string>("Slider", ["Item1", "Item2", "Item3"])
        {
            DisplayLabel = "IChoiceOption { Mode = Slider }",
            Mode = ChoiceOptionMode.Slider,
        });

        OptionDefinitions.Add(new ChoiceOption<string>("ToggleButtons", ["Item1", "Item2", "Item3"])
        {
            DisplayLabel = "IChoiceOption { Mode = ToggleButtons }",
            Mode = ChoiceOptionMode.ToggleButtons,
        });
    }
}
