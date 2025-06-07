using Microsoft.UI.Xaml;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Modules.App;

namespace Poltergeist.Helpers;

public static class ApplicationHelper
{
    public static void Exit()
    {
        App.TryEnqueue(() =>
        {
            Application.Current.Exit();
        });
    }

    public static void Minimize()
    {
        var hwnd = PoltergeistApplication.MainWindow.GetWindowHandle();
        var wh = new WindowHelper(hwnd);
        wh.Minimize();
    }

    public static void Restore()
    {
        var hwnd = PoltergeistApplication.MainWindow.GetWindowHandle();
        var wh = new WindowHelper(hwnd);
        wh.Unminimize();
    }

    public static void BringToFront()
    {
        Restore();
        PoltergeistApplication.MainWindow.BringToFront();
    }

    public static void Restart()
    {
        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
    }
}
