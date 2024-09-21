using Microsoft.UI.Xaml;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Helpers;

public static class ApplicationHelper
{
    public static void Exit()
    {
        Application.Current.Exit();
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
