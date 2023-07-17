using System.Collections.Generic;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Interactions;

public class InteractionCallbackArguments : ArgumentService
{
    public Dictionary<string, string>? Arguments { get; set; }
    public bool AllowsResume { get; set; }

    public InteractionCallbackArguments(MacroProcessor processor) : base(processor)
    {
    }
}
