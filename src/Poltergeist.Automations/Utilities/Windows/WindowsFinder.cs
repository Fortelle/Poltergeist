using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

public static class WindowsFinder
{
    public static Size GetScreenSize()
    {
        var width = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        var height = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        return new Size(width, height);
    }

    public static IntPtr FindWindow(string? windowName, string? className, string? processName)
    {
        var hWnd = IntPtr.Zero;
        if (processName is not null)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0 && windowName is not null)
            {
                foreach (var proc in processes)
                {
                    if (proc.MainWindowHandle != default)
                    {
                        hWnd = proc.MainWindowHandle;
                        break;
                    }
                }
            }
            else if (processes.Length == 1)
            {
                hWnd = processes[0].MainWindowHandle;
            }
        }
        else if (windowName is not null || className is not null)
        {
            hWnd = NativeMethods.FindWindow(className, windowName);
        }


        return hWnd;
    }

    public static IntPtr FindWindow(Func<IntPtr, bool> action)
    {
        var hwnd = IntPtr.Zero;
        NativeMethods.EnumWindows((h, l) => {
            if (action(h))
            {
                hwnd = h;
                return false;
            }
            else
            {
                return true;
            }
        }, IntPtr.Zero);

        return hwnd;
    }

    public static WindowHelper? FindWindow(Func<WindowHelper, bool> action)
    {
        WindowHelper? windowHelper = null;
        NativeMethods.EnumWindows((h, l) => {
            var wh = new WindowHelper(h);
            if (action(wh))
            {
                windowHelper = wh;
                return false;
            }
            else
            {
                return true;
            }
        }, IntPtr.Zero);
        return windowHelper;
    }


    public static IntPtr[] FindWindows(Func<IntPtr, bool> action)
    {
        var windows = new List<IntPtr>();
        NativeMethods.EnumWindows((h, l) => {
            if (action(h))
            {
                windows.Add(h);
            }
            return true;
        }, IntPtr.Zero);

        return windows.ToArray();
    }

    public static IntPtr[] FindChildWindows(IntPtr hwnd)
    {
        var parentHwnd = hwnd;
        var targetHwnd = IntPtr.Zero;
        var children = new List<IntPtr>();

        NativeMethods.EnumChildWindows(parentHwnd, (h, l) =>
        {
            children.Add(h);
            return true;
        }, IntPtr.Zero);

        return children.ToArray();
    }

    public static IntPtr FindChildWindow(IntPtr parentHwnd, string lpszClass)
    {
        var childHwnd = NativeMethods.FindWindowEx(parentHwnd, IntPtr.Zero, lpszClass, "");
        return childHwnd;
    }

    public static Process[] GetProcessesByPath(string path)
    {
        var processes = new List<Process>();
        var processName = Path.GetFileNameWithoutExtension(path);

        foreach (var process in Process.GetProcessesByName(processName))
        {
            if(process.MainModule?.FileName == path)
            {
                processes.Add(process);
            }
        }

        return processes.ToArray();
    }


    public static IntPtr GetForegroundWindow()
    {
        return NativeMethods.GetForegroundWindow();
    }

    private static class NativeMethods
    {
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}
