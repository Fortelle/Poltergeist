using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class ProcessSummaryCreatedHook : MacroHook
{
    public ProcessSummary Summary { get; }

    public ProcessSummaryCreatedHook(ProcessSummary summary)
    {
        Summary = summary;
    }
}