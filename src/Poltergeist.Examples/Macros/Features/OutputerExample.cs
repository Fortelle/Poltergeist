using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class OutputerExample : BasicMacro
{
    public OutputerExample() : base()
    {
        Title = nameof(OutputService);

        Category = "Features";

        Description = "This example shows how to print messages to the dashboard by using ArgumentService.Outputer.";

        Execute = (args) =>
        {
            for (var i = 0; i < 3; i++)
            {
                args.Outputer.NewGroup($"Group {i + 1}:");
                for (var j = 0; j < 3; j++)
                {
                    Thread.Sleep(100);
                    args.Outputer.Write($"Item {j + 1}");
                }
            }
        };
    }

}
