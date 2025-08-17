using Poltergeist.Automations.Processors;
using Poltergeist.Helpers;

namespace Poltergeist.Modules.App;

public class ActionService
{
    public ActionService()
    {
    }

    public void Execute(CompletionAction action, object? argument = null)
    {
        switch (action)
        {
            case CompletionAction.ExitApplication:
                ApplicationHelper.Exit();
                break;
            case CompletionAction.LockScreen:
                SystemHelper.LockScreen();
                break;
            case CompletionAction.ShutdownSystem:
                SystemHelper.Shutdown();
                ApplicationHelper.Exit();
                break;
            case CompletionAction.HibernateSystem:
                SystemHelper.Hibernate();
                break;
            case CompletionAction.LogOffSystem:
                SystemHelper.LogOff();
                ApplicationHelper.Exit();
                break;
            case CompletionAction.RestoreApplication:
                ApplicationHelper.BringToFront();
                break;
        }
    }

}
