using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class BooleanOptionExample : MacroBase
{
    public BooleanOptionExample() : base()
    {
        Title = "Boolean options";

        Category = "Options";

        Description = "This example defines several boolean options with different modes.";

        OptionDefinitions.Add(new OptionDefinition<bool>("bool")
        {
            DisplayLabel = @"OptionDefinition<bool>",
        });

        OptionDefinitions.Add(new BoolOption("bool_switch")
        {
            DisplayLabel = @"BoolOption { Mode = ToggleSwitch }",
            Mode = BoolOptionMode.ToggleSwitch,
            OnText = "True",
            OffText = "False",
        });

        OptionDefinitions.Add(new BoolOption("bool_checkbox")
        {
            DisplayLabel = @"BoolOption { Mode = CheckBox }",
            Mode = BoolOptionMode.CheckBox,
            Text = "Text",
        });

        OptionDefinitions.Add(new BoolOption("bool_buttons")
        {
            DisplayLabel = @"BoolOption { Mode = ToggleButtons }",
            Mode = BoolOptionMode.ToggleButtons,
            OnText = "True",
            OffText = "False",
        });

        OptionDefinitions.Add(new BoolOption("bool_leftright")
        {
            DisplayLabel = @"BoolOption { Mode = LeftRightSwitch }",
            Mode = BoolOptionMode.LeftRightSwitch,
            OnText = "True",
            OffText = "False",
        });

    }
}
