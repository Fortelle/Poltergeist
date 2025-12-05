using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Interactions;

public class InteractionService : MacroService
{
    public const string InteractionIdKey = "interaction_id";

    public static Func<InteractingEventArgs, Task>? Interacting { get; set; }

    private InteractionModel? InteractingModel;

    public InteractionService(MacroProcessor processor, HookService hookService) : base(processor)
    {
        hookService.Register<MessageReceivedHook>(OnMessageReturned);
    }

    public void Push(NotificationModel model)
    {
        model.ProcessorId = Processor.ProcessorId;

        var args = new InteractingEventArgs(model);
        Processor.RaiseEvent(ProcessorEvent.Interacting, args);
    }

    public async Task Interact(InteractionModel model)
    {
        if (InteractingModel is not null)
        {
            throw new InvalidOperationException("An interaction is already in progress. Please wait for it to complete before starting a new one.");
        }

        InteractingModel = model;
        model.ProcessorId = Processor.ProcessorId;

        var args = new InteractingEventArgs(model);
        Processor.RaiseEvent(ProcessorEvent.Interacting, args);

        await Processor.Pause(PauseReason.WaitForInput);
    }

    private void OnMessageReturned(MessageReceivedHook hook)
    {
        if (InteractingModel is null)
        {
            return;
        }
        if (!hook.Arguments.TryGetValue(InteractionIdKey, out var hookInteractionId))
        {
            return;
        }
        if (hookInteractionId != InteractingModel.Id)
        {
            throw new InvalidOperationException($"Interaction ID mismatch.");
        }

        InteractingModel = null;

        Processor.Resume();
    }

    public static void UIPush(NotificationModel model)
    {
        if (Interacting is null)
        {
            return;
        }

        var args = new InteractingEventArgs(model);
        Interacting.Invoke(args);
    }

    public static async Task UIInteract(InteractionModel model)
    {
        if (Interacting is null)
        {
            return;
        }

        var args = new InteractingEventArgs(model);
        await Interacting.Invoke(args);
    }
}
