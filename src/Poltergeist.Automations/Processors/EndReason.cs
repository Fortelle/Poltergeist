namespace Poltergeist.Automations.Processors;

public enum EndReason
{
    None,
    Unstarted,
    Purposed,
    UserAborted,
    SystemShutdown,// todo
    ErrorOccurred,
    Complete,
}
