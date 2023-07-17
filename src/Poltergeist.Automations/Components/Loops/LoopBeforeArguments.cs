using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopBeforeArguments : ArgumentService
{
    public bool Cancel { get; set; }

    public int CountLimit { get; set; }

    public LoopBeforeArguments(MacroProcessor processor) : base(processor)
    {
    }
}
