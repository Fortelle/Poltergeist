using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components;

public class CompleteModule : MacroModule
{

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.OptionDefinitions.Add(new EnumOption<CompletionAction>("aftercompletion.action", CompletionAction.None)
        {
            DisplayLabel = LocalizationUtil.Localize("AfterCompletion_Option_Action"),
            Category = LocalizationUtil.Localize("AfterCompletion_Category"),
            GetText = x => LocalizationUtil.Localize($"AfterCompletion_CompletionAction_{x}"),
        });

        macro.OptionDefinitions.Add(new OptionDefinition<TimeOnly>("aftercompletion.minimumtime")
        {
            DisplayLabel = LocalizationUtil.Localize("AfterCompletion_Option_MinimumTime"),
            Category = LocalizationUtil.Localize("AfterCompletion_Category"),
            Description = LocalizationUtil.Localize("AfterCompletion_Option_MinimumTime_Description"),
        });

        macro.OptionDefinitions.Add(new OptionDefinition<bool>("aftercompletion.allowerror")
        {
            DisplayLabel = LocalizationUtil.Localize("AfterCompletion_Option_AllowError"),
            Category = LocalizationUtil.Localize("AfterCompletion_Category"),
            Description = LocalizationUtil.Localize("AfterCompletion_Option_AllowError_Description"),
        });
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        processor.Hooks.Register<ProcessorEndingHook>(OnProcessorEnding);
    }

    private void OnProcessorEnding(ProcessorEndingHook hook)
    {
        var completeAction = hook.Processor.Options.GetValueOrDefault<CompletionAction>("aftercompletion.action");
        var completeAllowerror = hook.Processor.Options.GetValueOrDefault<bool>("aftercompletion.allowerror");
        var completeMinimumTime = hook.Processor.Options.GetValueOrDefault<TimeOnly>("aftercompletion.minimumtime");

        if (hook.Reason == EndReason.Interrupted)
        {
            completeAction = CompletionAction.None;
        }
        else if (hook.Reason == EndReason.ErrorOccurred && !completeAllowerror)
        {
            completeAction = CompletionAction.None;
        }
        else if (completeMinimumTime != default && completeMinimumTime.ToTimeSpan().Ticks < hook.Duration.Ticks)
        {
            completeAction = CompletionAction.None;
        }
        else if (hook.Reason != EndReason.Complete)
        {
            completeAction = CompletionAction.None;
        }

        if (completeAction != CompletionAction.None)
        {
            hook.OutputStorage.TryAdd("complete_action", completeAction);
        }
    }

}
