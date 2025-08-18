using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples;

[ExampleMacro]
public class GlitchMacro : MacroBase
{
    public GlitchMacro() : base()
    {
        Title = "Glitch Macro";

        Category = "Macros";

        Description = "This is a glitch macro.";

        Exception = new Exception("This is a glitch exception.");
    }
}
