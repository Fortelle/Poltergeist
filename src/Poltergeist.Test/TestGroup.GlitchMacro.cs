using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public class GlitchMacro : MacroBase
    {
        public GlitchMacro() : base("test_glitch")
        {
            Title = nameof(GlitchMacro);
            Description = "This is a glitch macro that can never be accessed.";
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            throw new Exception();
        }
    }
}