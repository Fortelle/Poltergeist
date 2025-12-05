namespace Poltergeist.Automations.Processors;

public enum CompletionAction
{
    None,
    RestoreApplication,
    ExitApplication,

    LockScreen,

    ShutdownSystem,
    RestartSystem,
    HibernateSystem,
    LogOffSystem,

    NotifyMe,
    //todo:RunMacro,
}
