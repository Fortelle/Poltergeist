using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Processors;
using Poltergeist.Helpers;
using Poltergeist.Modules.Navigation;
using Poltergeist.UI.Pages.Macros;

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

    public static void RunMacro(LaunchReason reason, bool toggle)
    {
        var navigationService = PoltergeistApplication.GetService<INavigationService>();
        if (navigationService.TabView!.SelectedItem is not TabViewItem tabViewItem)
        {
            return;
        }

        if (tabViewItem.Content is not MacroPage macroPage)
        {
            return;
        }

        if (macroPage.ViewModel!.IsRunning)
        {
            if (toggle)
            {
                macroPage.ViewModel.Stop();
            }
            else
            {
                PoltergeistApplication.ShowTeachingTip(PoltergeistApplication.Localize($"Poltergeist/Macro/MacroAlreadyRunning", macroPage.ViewModel.Shell.Title));
                return;
            }
        }
        else
        {
            macroPage.ViewModel.Start(new()
            {
                ShellKey = macroPage.ViewModel.Shell.ShellKey,
                Reason = reason,
            });
        }
    }

    public static void RunMacro(MacroStartArguments args)
    {
        var pageKey = "macro:" + args.ShellKey;

        var navigationService = PoltergeistApplication.GetService<INavigationService>();
        if (!navigationService.NavigateTo(pageKey))
        {
            return;
        }
        if (navigationService.TabView!.SelectedItem is not TabViewItem tabViewItem)
        {
            return;
        }
        if (tabViewItem.Tag.ToString() != pageKey)
        {
            return;
        }
        if (tabViewItem.Content is not MacroPage macroPage)
        {
            return;
        }
        if (macroPage.ViewModel!.IsRunning)
        {
            PoltergeistApplication.ShowTeachingTip(PoltergeistApplication.Localize($"Poltergeist/Macro/MacroAlreadyRunning", macroPage.ViewModel.Shell.Title));
            return;
        }
        else
        {
            macroPage.ViewModel.Start(args);
        }
    }

}
