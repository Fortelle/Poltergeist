using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.UI.Controls;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ParameterDefinitionStatusExample : UnrunnableMacro
{
    public ParameterDefinitionStatusExample() : base()
    {
        Title = "Status icons";

        Category = "Options";

        Description = $"This example uses the {nameof(ParameterDefinitionBase.Status)} property to demonstrate the status icons.";

        foreach (var value in Enum.GetValues<ParameterStatus>())
        {
            OptionDefinitions.Add(new ParameterDefinition<bool>($"{value}")
            {
                Description = $"{{ {nameof(ParameterDefinitionBase.Status)} = {value} }}",
                Status = value,
            });
        }
    }
}
