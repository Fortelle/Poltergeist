using Poltergeist.Automations.Common;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Components.Loops;

public class CompleteModule : MacroModule
{

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.UserOptions.Add(new EnumOption<CompletionAction>("aftercompletion.action", CompletionAction.None)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_Action"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            GetText = x => ResourceHelper.Localize($"Poltergeist.Automations/Resources/AfterCompletion_CompletionAction_{x}"),
        });

        macro.UserOptions.Add(new OptionItem<int>("aftercompletion.minimumseconds", 0)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_MinimumTime"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_MinimumTime_Description"),
        });

        macro.UserOptions.Add(new OptionItem<bool>("aftercompletion.allowerror", false)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_AllowError"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_AllowError_Description"),
        });
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        processor.Hooks.Register<ProcessorEndingHook>(OnProcessorEnding);
    }

    private void OnProcessorEnding(ProcessorEndingHook hook, IServiceProcessor processor)
    {
        var completeAction = processor.Options.Get("aftercompletion.action", hook.CompletionAction);
        var completeAllowerror = processor.Options.Get("aftercompletion.allowerror", false);
        var completeMinimumSeconds = processor.Options.Get("aftercompletion.minimumseconds", 0);

        if (hook.Reason is EndReason.UserAborted or EndReason.LoadFailed or EndReason.Unstarted)
        {
            completeAction = CompletionAction.None;
        }
        else if (hook.Reason == EndReason.ErrorOccurred && !completeAllowerror)
        {
            completeAction = CompletionAction.None;
        }
        else if (completeMinimumSeconds > 0 && completeMinimumSeconds < hook.HistoryEntry.Duration.TotalSeconds)
        {
            completeAction = CompletionAction.None;
        }

        hook.CompletionAction = completeAction;
    }

}
