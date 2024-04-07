using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

[AutoLoad]
public partial class TestGroup : MacroGroup
{
    public TestGroup() : base("Tests")
    {
        Description = "Provides a series of macros for testing.";
    }
}
