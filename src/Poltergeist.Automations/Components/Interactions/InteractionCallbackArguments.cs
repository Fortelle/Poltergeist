using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Interactions;

public class InteractionCallbackArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public Dictionary<string, string>? Arguments { get; set; }
    public bool AllowsResume { get; set; }
}
