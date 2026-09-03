using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components;

// todo: convert to ui level option
public class CompleteModule : MacroModule
{

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

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

    [MacroHook]
    public static void OnProcessorCompleted(IMacroProcessorShared processor, ProcessorCompletedHook hook)
    {
        var completeAction = processor.Options.GetValueOrDefault<CompletionAction>("aftercompletion.action");
        var completeAllowerror = processor.Options.GetValueOrDefault<bool>("aftercompletion.allowerror");
        var completeMinimumTime = processor.Options.GetValueOrDefault<TimeOnly>("aftercompletion.minimumtime");
        
        if (hook.Conclusion == ProcessorConclusion.Crushed)
        {
            completeAction = CompletionAction.None;
        }
        else if (hook.Conclusion == ProcessorConclusion.Canceled || hook.Conclusion == ProcessorConclusion.Terminated)
        {
            completeAction = CompletionAction.None;
        }
        else if (hook.Conclusion == ProcessorConclusion.ErrorOccurred && !completeAllowerror)
        {
            completeAction = CompletionAction.None;
        }
        else if (hook.Conclusion == ProcessorConclusion.Failure)
        {
            completeAction = CompletionAction.None;
        }
        else if (completeMinimumTime != default && completeMinimumTime.ToTimeSpan().Ticks < hook.Duration.Ticks)
        {
            completeAction = CompletionAction.None;
        }

        if (completeAction != CompletionAction.None)
        {
            hook.OutputStorage.TryAdd("complete_action", completeAction);
        }
    }
}
