using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

[AutoLoad]
public partial class TestGroup : MacroGroup
{
    public TestGroup() : base("Tests")
    {
        Description = "Provides a series of macros for testing.";
        Options = new()
        {
            new OptionItem<int>("test_option_a", 1),
            new OptionItem<int>("test_option_c", 1),
        };
    }

    public override void SetGlobalOptions(MacroOptions options)
    {
        options.Add<int>("test_option_a", 0);
        options.Add<int>("test_option_b", 0);
    }

}
