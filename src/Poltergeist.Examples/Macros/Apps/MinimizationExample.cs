using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MinimizationExample : BasicMacro
{
    public MinimizationExample() : base()
    {
        Title = "Minimization";

        Category = "Apps";

        Description = "A macro that minimizes the application window while running.";

        Modules.Add(new MinimizationModule(true));

        Execute = (args) =>
        {
            Thread.Sleep(3000);
        };

    }
}
