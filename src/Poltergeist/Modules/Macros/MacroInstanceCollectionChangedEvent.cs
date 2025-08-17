using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroInstanceCollectionChangedEvent : AppEvent
{
    public MacroInstance[]? NewItems { get; init; }
    public MacroInstance[]? OldItems { get; init; }
}
