namespace Poltergeist.Automations.Components.Loops;

public enum IterationResult
{
    Undetermined,
    Continue,
    Break,
    ForceContinue,
    Error,
    Failed,
    Interrupted,

    RestartLoop,
    RestartIteration,
}
