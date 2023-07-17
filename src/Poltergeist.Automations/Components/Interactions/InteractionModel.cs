using System;

namespace Poltergeist.Automations.Components.Interactions;

public abstract class InteractionModel
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string? MacroKey { get; set; }
    public string? ProcessId { get; set; }
    
    public virtual void Callback(InteractionCallbackArguments args)
    {

    }
}
