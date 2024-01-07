namespace Poltergeist.Automations.Processors;

public enum EndReason
{
    None,
    InitializationFailed, // not be accessible
    LoadFailed,
    Unstarted,
    UserAborted,
    SystemShutdown, // todo
    ErrorOccurred,
    Empty,
    Complete,
}
