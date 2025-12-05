using System.Diagnostics;

namespace Poltergeist.Helpers;

public static class SystemHelper
{
    public static void LockScreen()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "rundll32",
            Arguments = "user32.dll,LockWorkStation",
            CreateNoWindow = true,
        });
    }

    public static void Shutdown()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/s /d p:4:1",
            CreateNoWindow = true,
        });
    }

    // todo: toast notification (cancellable)
    public static void Restart()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "shutdown",
            Arguments = "/r /d p:4:1",
            CreateNoWindow = true,
        });
    }

    public static void Hibernate()
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

    public static void LogOff()
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

}
