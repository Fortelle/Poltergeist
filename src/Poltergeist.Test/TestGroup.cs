using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Test;

[AutoLoad]
public partial class TestGroup : MacroGroup
{
    public TestGroup() : base("Tests")
    {
        Description = "Provides a series of macros for testing.";
        Options.Add(new OptionItem<int>("test_option_a", 1));
        Options.Add(new OptionItem<int>("test_option_c", 1));
    }

}
