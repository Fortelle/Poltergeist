﻿using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components;

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

        macro.UserOptions.Add(new OptionDefinition<TimeOnly>("aftercompletion.minimumtime")
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_MinimumTime"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_MinimumTime_Description"),
        });

        macro.UserOptions.Add(new OptionDefinition<bool>("aftercompletion.allowerror")
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

    private void OnProcessorEnding(ProcessorEndingHook hook, IUserProcessor processor)
    {
        var completeAction = processor.Options.Get<CompletionAction>("aftercompletion.action");
        var completeAllowerror = processor.Options.Get<bool>("aftercompletion.allowerror");
        var completeMinimumTime = processor.Options.Get<TimeOnly>("aftercompletion.minimumtime");

        if (hook.Reason is EndReason.None or EndReason.InitializationFailed or EndReason.LoadFailed or EndReason.Unstarted or EndReason.SystemShutdown)
        {
            hook.CompletionAction = CompletionAction.None;
        }
        else if (hook.Reason == EndReason.UserAborted)
        {
            hook.CompletionAction = CompletionAction.None;
        }
        else if (hook.Reason == EndReason.ErrorOccurred && !completeAllowerror)
        {
            hook.CompletionAction = CompletionAction.None;
        }
        else if (completeMinimumTime != default && completeMinimumTime.ToTimeSpan().Ticks < hook.Duration.Ticks)
        {
            hook.CompletionAction = CompletionAction.None;
        }
        else
        {
            hook.CompletionAction = completeAction;
        }
    }

}
