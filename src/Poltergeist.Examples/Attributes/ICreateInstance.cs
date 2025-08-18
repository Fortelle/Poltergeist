using Poltergeist.Automations.Macros;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Examples;

public interface ICreateInstance
{
    MacroInstance CreateInstance(MacroBase template);
}
