using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{

    [AutoLoad]
    public class MinimalMacro : MacroBase
    {
        public MinimalMacro() : base("test_minimal")
        {
            Title = nameof(MinimalMacro);
            Description = "This is an empty macro with the minimum dependencies.";
            IsSingleton = true;
        }
    }

}
