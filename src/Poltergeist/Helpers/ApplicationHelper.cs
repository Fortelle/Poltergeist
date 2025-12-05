using Microsoft.UI.Xaml;
using Poltergeist.Automations.Utilities.Windows;

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

    public static void Maximize()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        WindowUtil.Maximize(hwnd);
    }

    public static void Minimize()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        WindowUtil.Minimize(hwnd);
    }

    public static void Restore()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        WindowUtil.Restore(hwnd);
    }

    public static void BringToFront()
    {
        Restore();
        PoltergeistApplication.Current.MainWindow.BringToFront();
    }

    public static bool IsMinimize()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        return WindowUtil.IsMinimized(hwnd);
    }

    public static bool IsForeground()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        return WindowUtil.IsForeground(hwnd);
    }

    public static void Restart()
    {
        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
    }
}
