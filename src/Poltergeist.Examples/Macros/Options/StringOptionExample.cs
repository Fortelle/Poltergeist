using System.Text.RegularExpressions;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class StringOptionExample : UnrunnableMacro
{
    public StringOptionExample() : base()
    {
        Title = "String options";

        Category = "Options";

        Description = "This example defines several string options with different modes.";

        OptionDefinitions.Add(new OptionDefinition<string>("string")
        {
            Description = "OptionDefinition<string>",
        });

        OptionDefinitions.Add(new OptionDefinition<string[]>("string[]")
        {
            Description = "OptionDefinition<string[]>",
        });

        OptionDefinitions.Add(new TextOption("validation")
        {
            Description = "TextOption { Valid = s => Regex.IsMatch(s, @\"^\\d+$\") }",
            Valid = s => Regex.IsMatch(s, @"^\d+$"),
        });

        OptionDefinitions.Add(new TextOption("multiline")
        {
            Description = "TextOption { Multiline = true }",
            Multiline = true,
        });

        OptionDefinitions.Add(new PasswordOption("password")
        {
            Description = "PasswordOption",
        });
    }
}
