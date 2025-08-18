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
        var wh = new WindowHelper(hwnd);
        wh.Maximize();
    }

    public static void Minimize()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        var wh = new WindowHelper(hwnd);
        wh.Minimize();
    }

    public static void Restore()
    {
        var hwnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        var wh = new WindowHelper(hwnd);
        wh.Restore();
    }

    public static void BringToFront()
    {
        Restore();
        PoltergeistApplication.Current.MainWindow.BringToFront();
    }

    public static void Restart()
    {
        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
    }
}
