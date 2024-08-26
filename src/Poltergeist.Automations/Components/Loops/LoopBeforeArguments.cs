using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopBeforeArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public bool Cancel { get; set; }

    public int CountLimit { get; set; }
}
