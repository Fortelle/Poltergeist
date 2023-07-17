using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

#if DEBUG
[AutoLoad]
#endif
public class DevGroup : MacroGroup
{
    public DevGroup() : base("Dev")
    {
    }

    [AutoLoad]
    public BasicMacro DevMacro = new("dev_macro")
    {
    };

}
