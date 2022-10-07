using System.Diagnostics;
using System.Windows;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Services;

public class ActionService
{
    public ActionService()
    {
    }

    public void Execute(CompleteAction action, object argument)
    {
        switch (action)
        {
            case CompleteAction.ExitApplication:
                Application.Current.Shutdown();
                break;
            case CompleteAction.LockScreen:
                LockScreen();
                break;
            case CompleteAction.ShutdownSystem:
                ShutdownSystem();
                break;
            case CompleteAction.HibernateSystem:
                HibernateSystem();
                break;
            case CompleteAction.LogOffSystem:
                LogOffSystem();
                break;
        }
    }

    public void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    public void RestartApplication()
    {
    }

    public void MinimizeApplication()
    {
    }

    public void RestoreApplication()
    {
    }

    public void LockScreen()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "rundll32",
            Arguments = "user32.dll,LockWorkStation",
            CreateNoWindow = true,
        });
    }

    public void ShutdownSystem()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/s /t 30 /d p:4:1",
            CreateNoWindow = true,
        });
        ExitApplication();
    }

    public void RestartSystem()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/r /d p:4:1",
            CreateNoWindow = true,
        });
        ExitApplication();
    }

    public void HibernateSystem()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/h",
            CreateNoWindow = true,
        });
    }

    public void LogOffSystem()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/l",
            CreateNoWindow = true,
        });
    }
}
