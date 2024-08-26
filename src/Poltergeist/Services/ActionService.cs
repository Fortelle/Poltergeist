using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Contracts.Services;
using Poltergeist.Pages.Macros;

namespace Poltergeist.Services;

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
                ExitApplication();
                break;
            case CompletionAction.LockScreen:
                LockScreen();
                break;
            case CompletionAction.ShutdownSystem:
                ShutdownSystem();
                break;
            case CompletionAction.HibernateSystem:
                HibernateSystem();
                break;
            case CompletionAction.LogOffSystem:
                LogOffSystem();
                break;
            case CompletionAction.RestoreApplication:
                RestoreApplication();
                break;
        }
    }

    public static void RunMacro(LaunchReason reason, bool toggle)
    {
        var navigationService = App.GetService<INavigationService>();
        if (navigationService.TabView!.SelectedItem is not TabViewItem tabViewItem)
        {
            return;
        }

        if(tabViewItem.Content is not MacroPage macroPage)
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
                App.ShowTeachingTip(App.Localize($"Poltergeist/Macro/MacroAlreadyRunning", macroPage.ViewModel.Shell.Title));
                return;
            }
        }
        else
        {
            macroPage.ViewModel.Start(new(){
                ShellKey = macroPage.ViewModel.Shell.ShellKey,
                Reason = reason,
            });
        }
    }

    public static void RunMacro(MacroStartArguments args)
    {
        var pageKey = "macro:" + args.ShellKey;

        var navigationService = App.GetService<INavigationService>();
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
            App.ShowTeachingTip(App.Localize($"Poltergeist/Macro/MacroAlreadyRunning", macroPage.ViewModel.Shell.Title));
            return;
        }
        else
        {
            macroPage.ViewModel.Start(args);
        }
    }

    public static void ExitApplication()
    {
        Application.Current.Exit();
    }

    public static void MinimizeApplication()
    {
        var hwnd = App.MainWindow.GetWindowHandle();
        var wh = new WindowHelper(hwnd);
        wh.Minimize();
    }

    public static void RestoreApplication()
    {
        var hwnd = App.MainWindow.GetWindowHandle();
        var wh = new WindowHelper(hwnd);
        wh.Unminimize();
    }

    public static void BringToFront()
    {
        RestoreApplication();
        App.MainWindow.BringToFront();
    }

    public static void LockScreen()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "rundll32",
            Arguments = "user32.dll,LockWorkStation",
            CreateNoWindow = true,
        });
    }

    public static void ShutdownSystem()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/s /t 30 /d p:4:1",
            CreateNoWindow = true,
        });
        ExitApplication();
    }

    // todo: toast notification (cancellable)
    public static void RestartSystem()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/r /d p:4:1",
            CreateNoWindow = true,
        });
        ExitApplication();
    }

    public static void HibernateSystem()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/h",
            CreateNoWindow = true,
        });
    }

    public static void LogOffSystem()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/l",
            CreateNoWindow = true,
        });
    }

    public static void RestartApplication()
    {
        throw new NotImplementedException();
    }
}
