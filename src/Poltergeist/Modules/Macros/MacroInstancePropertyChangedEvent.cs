using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroInstancePropertyChangedEvent(MacroInstance instance) : AppEvent
{
    public MacroInstance Instance => instance;
}
