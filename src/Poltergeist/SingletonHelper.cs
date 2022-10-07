using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using Poltergeist.Input.Windows;

namespace Poltergeist;

internal static class SingletonHelper
{
    private const string AppKey = "Poltergeist";
    private const string MutexKey = $"{AppKey}_Singleton_Mutex";
    private const string PropKey = $"{AppKey}_Specificity_Prop";

    private static readonly Mutex mutex = new(true, MutexKey);

    public static bool IsSingleInstance => mutex.WaitOne(TimeSpan.Zero);

    public static void Load(MainWindow window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var source = HwndSource.FromHwnd(hwnd);
        source.AddHook(new(WndProc));

        NativeMethods.SetProp(hwnd, PropKey, (IntPtr)1);
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_COPYDATA)
        {
            ShowWindow(hwnd);
            try
            {
                var data = Marshal.PtrToStructure<NativeMethods.COPYDATASTRUCT>(lParam);
                var text = Marshal.PtrToStringAnsi(data.lpData, data.cbData);
                var args = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                App.ParseArguments(args);
                handled = true;
            }
            catch (Exception) { }
        }
        return IntPtr.Zero;
    }

    public static void SendData(string[] args)
    {
        var singletonHwnd = IntPtr.Zero;
        NativeMethods.EnumWindows((hwnd, _) => {
            var value = NativeMethods.GetProp(hwnd, PropKey).ToInt32();
            if (value == 1)
            {
                singletonHwnd = hwnd;
                return false;
            }
            else
            {
                return true;
            }
        }, IntPtr.Zero);
        if (singletonHwnd == default) return;

        var message = string.Join('\n', args);
        var data = new NativeMethods.COPYDATASTRUCT()
        {
            dwData = IntPtr.Zero,
            cbData = message.Length,
            lpData = Marshal.StringToHGlobalAnsi(message),
        };
        NativeMethods.SendMessage(singletonHwnd, NativeMethods.WM_COPYDATA, IntPtr.Zero, ref data);
    }

    private static void ShowWindow(IntPtr hwnd)
    {
        if (NativeMethods.IsIconic(hwnd))
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
        }
        NativeMethods.SetForegroundWindow(hwnd);
    }

    private static class NativeMethods
    {
        public const int WM_COPYDATA = 0x004A;
        public const int SW_RESTORE = 9;

        [DllImport("USER32.DLL")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("USER32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [DllImport("USER32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetProp(IntPtr hWnd, string lpString, IntPtr hData);

        [DllImport("USER32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        [DllImport("USER32.dll")]
        public static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

    }
}
