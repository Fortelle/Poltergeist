namespace Poltergeist.Automations.Processors;

public enum WorkflowStepState
{
    Idle,
    InitiallySuccess,
    InitiallyError,

    Success,
    Failed,
    Error,
    Interrupted,
}
