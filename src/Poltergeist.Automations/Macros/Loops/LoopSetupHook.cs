using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopSetupHook : MacroHook
{
    public LoopPattern Pattern { get; set; }
    public int MaxIterations { get; set; }
    public TimeSpan MaxDuration { get; set; }

    public StartCallback? StartAsync { get; set; }
    public ExecuteCallback? ExecuteAsync { get; set; }
    public TransitionCallback? TransitionAsync { get; set; }
    public EndCallback? EndAsync { get; set; }
    public FinalizeCallback? Finalize { get; set; }
}
