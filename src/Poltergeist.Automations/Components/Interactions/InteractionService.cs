using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Interactions;

public class InteractionService : MacroService
{
    public const string InteractionIdKey = "interaction_id";

    public static Func<InteractingEventArgs, Task>? Interacting;

    // todo: convert to callback parameter
    private List<InteractionModel> Models { get; } = new();

    public InteractionService(MacroProcessor processor, HookService hookService) : base(processor)
    {
        hookService.Register<MessageReceivedHook>(OnMessageReturned);
    }

    public void Show(InteractionModel model)
    {
        model.MacroKey = Processor.Macro.Key;
        model.ProcessId = Processor.ProcessId;
        Models.Add(model);

        var args = new InteractingEventArgs(model);
        Processor.RaiseEvent(MacroEventType.Interacting, args);

        Logger.Debug($"Passed {nameof(InteractionModel)} <{model.GetType().Name}> to UI thread.", model);

    }

    public async Task ShowAsync(InteractionModel model)
    {
        model.MacroKey = Processor.Macro.Key;
        model.ProcessId = Processor.ProcessId;
        Models.Add(model);

        var args = new InteractingEventArgs(model)
        {
            Suspended = true
        };
        Processor.RaiseEvent(MacroEventType.Interacting, args);

        Logger.Debug($"Passed {nameof(InteractionModel)} <{model.GetType().Name}> to UI thread.", model);

        await Processor.Pause();
    }

    public void ShowTip(string message)
    {
        var model = new TipModel()
        {
            Text = message,
        };

        Show(model);
    }

    private void OnMessageReturned(MessageReceivedHook hook)
    {
        if (!hook.Arguments.TryGetValue(InteractionIdKey, out var interactionId))
        {
            return;
        }

        var model = Models.FirstOrDefault(x => x.Id == interactionId);
        if (model is null)
        {
            return;
        }

        var callbackArguments = Processor.GetService<InteractionCallbackArguments>();
        callbackArguments.Arguments = hook.Arguments;
        model.Callback(callbackArguments);

        if (callbackArguments.AllowsResume)
        {
            Processor.Resume();
        }
    }

    public static async Task UIShowAsync(InteractionModel model)
    {
        var args = new InteractingEventArgs(model)
        {
            Suspended = true
        };

        await Interacting!.Invoke(args);
    }
}
