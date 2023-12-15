using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class ProcessHistoryCreatedHook : MacroHook
{
    public ProcessHistoryEntry Entry { get; }

    public ProcessHistoryCreatedHook(ProcessHistoryEntry entry)
    {
        Entry = entry;
    }
}