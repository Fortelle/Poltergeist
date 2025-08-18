using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class PathOptionExample : UnrunnableMacro
{
    public PathOptionExample() : base()
    {
        Title = nameof(PathOption);

        Category = "Options";

        Description = $"This example defines several {nameof(PathOption)} with different modes.";

        foreach (var value in Enum.GetValues<PathOptionMode>())
        {
            OptionDefinitions.Add(new PathOption($"{value}")
            {
                Description = $"PathOption {{ {nameof(PathOption.Mode)} = {value} }}",
                Mode = value,
            });
        }
    }
}
