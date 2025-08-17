namespace Poltergeist.Automations.Components.Interactions;

public abstract class InteractionModel
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string? ProcessorId { get; set; }
    
    public virtual void Callback(InteractionCallbackArguments args)
    {

    }
}
