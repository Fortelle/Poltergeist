using Poltergeist.Automations.Macros;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroCollectionChangedHandler() : AppEventHandler
{
    public MacroShell[]? NewItems { get; init; }
    public MacroShell[]? OldItems { get; init; }
}
