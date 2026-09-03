using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Oneshots;

public class OneshotVisualizationModule : MacroModule
{
    public override bool Validate(IMacroProcessorInformation processor)
    {
        return processor.SessionStorage.GetValueOrDefault("oneshot_visualization_disabled", false) == false;
    }

    [MacroHook]
    public static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        var ph = processor.GetService<ProgressListInstrument>();
        ph.Key = "oneshot_visualization_instrument";
        ph.Title = "Status:";
        ph.Add(new(ProgressStatus.Busy)
        {
            Text = "Running",
        });
        processor.GetService<DashboardService>().Add(ph);
    }

    [MacroHook]
    public static void OnProcessorCompleted(IMacroProcessorShared processor, ProcessorCompletedHook hook)
    {
        var ph = processor.GetService<DashboardService>().Get<ProgressListInstrument>("oneshot_visualization_instrument");
        var status = hook.Conclusion switch
        {
            ProcessorConclusion.Success => ProgressStatus.Success,
            ProcessorConclusion.Canceled => ProgressStatus.Warning,
            ProcessorConclusion.Failure => ProgressStatus.Failure,
            ProcessorConclusion.ErrorOccurred => ProgressStatus.Failure,
            _ => ProgressStatus.Idle,
        };
        ph.Update(0, new(status)
        {
            Text = hook.Conclusion.ToString(),
        });
    }
}
