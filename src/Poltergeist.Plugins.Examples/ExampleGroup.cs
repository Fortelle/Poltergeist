using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

[AutoLoad]
public partial class ExampleGroup : MacroGroup
{
    public ExampleGroup() : base("Examples")
    {
        Description = "Provides a series of examples.";

    }

}
