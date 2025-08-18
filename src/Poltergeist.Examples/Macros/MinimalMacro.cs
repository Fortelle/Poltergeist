using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples;

[ExampleMacro]
public class MinimalMacro : MacroBase
{
    public MinimalMacro() : base()
    {
        Title = "Minimal Macro";

        Category = "Macros";

        Description = "An empty macro with the minimum dependencies.";
    }
}
