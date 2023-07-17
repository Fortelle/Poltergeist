namespace Poltergeist.Automations.Components.Loops;

public enum IterationStatus
{
    None,
    Continue,
    Stop,
    ForceContinue,
    Error,
    UserAborted,

    RestartLoop,
    RestartIteration,
}
