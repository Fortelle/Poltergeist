namespace Poltergeist.Automations.Processors;

public enum AbortReason
{
    Unknown,
    User,
    Error,
    Timeout,

#if DEBUG
    Test,
#endif
}
