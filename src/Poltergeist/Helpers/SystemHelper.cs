using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Poltergeist.Helpers;

public static partial class SystemHelper
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

    public static void PreventSleep(bool continuous, bool keepDisplayOn)
    {
        var flags = NativeMethods.ExecutionStates.SystemRequired;
        if (continuous)
        {
            flags |= NativeMethods.ExecutionStates.Continuous;
            // Only works on current thread. The prevention will end when the thread exits.
        }
        if (keepDisplayOn)
        {
            flags |= NativeMethods.ExecutionStates.DisplayRequired;
        }
        NativeMethods.SetThreadExecutionState(flags);
    }

    public static void AllowSleep()
    {
        NativeMethods.SetThreadExecutionState(NativeMethods.ExecutionStates.Continuous);
    }

    private static partial class NativeMethods
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial ExecutionStates SetThreadExecutionState(ExecutionStates esFlags);

        [Flags]
        public enum ExecutionStates : uint
        {
            Continuous = 0x80000000,
            SystemRequired = 0x00000001,
            DisplayRequired = 0x00000002,
            AwaymodeRequired = 0x00000040,
        }
    }
}