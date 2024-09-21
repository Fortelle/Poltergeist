using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

// the class must be created in the UI thread, otherwise the wndProc will not receive any keyboard input.
// https://github.com/dotnet/maui/blob/main/src/Compatibility/Core/src/WPF/Microsoft.Windows.Shell/Standard/NativeMethods.cs
// https://github.com/dotnet/maui/blob/main/src/Compatibility/Core/src/WPF/Microsoft.Windows.Shell/Standard/MessageWindow.cs
public class HotKeyListener : IDisposable
{
    public delegate void HotkeyPressHandler(HotKey hotkey);
    public event HotkeyPressHandler? HotkeyPressed;

    protected bool IsDisposed;

    private readonly nint HWnd;
    private readonly NativeMethods.WndProcDelegate? WndProc;
    private readonly string ClassName = "HotKeyClass_" + Guid.NewGuid().ToString();
    private readonly string WindowName = "HotKeyWindow";
    private readonly Dictionary<HotKey, int> HotKeyList = new();
    private int IdCounter;

    public HotKeyListener()
    {
        WndProc = new NativeMethods.WndProcDelegate(WindowProc);

        var wndClassEx = new NativeMethods.WNDCLASSEX
        {
            cbSize = Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX)),
            style = 0,
            cbClsExtra = 0,
            cbWndExtra = 0,
            hbrBackground = nint.Zero,
            hCursor = nint.Zero,
            hIcon = nint.Zero,
            hIconSm = nint.Zero,
            lpszClassName = ClassName,
            lpszMenuName = string.Empty,
            hInstance = NativeMethods.GetModuleHandle(string.Empty),
            lpfnWndProc = WndProc,
        };

        var atom = NativeMethods.RegisterClassEx(ref wndClassEx);
        if (atom == 0)
        {
            throw new Win32Exception();
        }

        HWnd = NativeMethods.CreateWindowEx(
           NativeMethods.WS_EX_NOACTIVATE,
           ClassName,
           WindowName,
           0,
           0,
           0,
           0,
           0,
           nint.Zero,
           nint.Zero,
           nint.Zero,
           nint.Zero
        );

        if (HWnd == nint.Zero)
        {
            throw new Win32Exception();
        }
    }

    public void Register(HotKey hotkey)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (HotKeyList.ContainsKey(hotkey))
        {
            throw new Exception($"The hot key '{hotkey}' is already registered.");
        }

        var id = Interlocked.Increment(ref IdCounter);

        var isSucceeded = NativeMethods.RegisterHotKey(HWnd, id, (uint)hotkey.Modifiers, (uint)hotkey.KeyCode);
        if (!isSucceeded)
        {
            throw new Win32Exception();
        }

        HotKeyList.Add(hotkey, id);
    }

    public void Unregister(HotKey hotkey)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!HotKeyList.TryGetValue(hotkey, out var id))
        {
            throw new Exception($"The hot key '{hotkey}' is not registered.");
        }

        var isSucceeded = NativeMethods.UnregisterHotKey(HWnd, id);
        if (!isSucceeded)
        {
            throw new Win32Exception();
        }

        HotKeyList.Remove(hotkey);
    }

    private nint WindowProc(nint hwnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            var id = (int)wParam;
            var key = lParam;
            var hotkey = HotKeyList.First(x => x.Value == id).Key;
            OnHotKeyPressed(hotkey);
            return nint.Zero;
        }
        else
        {
            return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }

    private void OnHotKeyPressed(HotKey hotkey)
    {
        HotkeyPressed?.Invoke(hotkey);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        foreach (var hotkey in HotKeyList.Keys)
        {
            try
            {
                Unregister(hotkey);
            }
            catch { }
        }
        NativeMethods.DestroyWindow(HWnd);
        NativeMethods.UnregisterClass(ClassName, NativeMethods.GetModuleHandle(string.Empty));

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~HotKeyListener()
    {
        Dispose(false);
    }

    private static class NativeMethods
    {
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(nint hWnd, int id);

        /*****/

        public const int WS_EX_NOACTIVATE = 0x08000000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public WndProcDelegate lpfnWndProc; // not WndProc
            public int cbClsExtra;
            public int cbWndExtra;
            public nint hInstance;
            public nint hIcon;
            public nint hCursor;
            public nint hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public nint hIconSm;
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassExW")]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterClass(string lpClassName, nint hInstance);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
        public static extern nint CreateWindowEx(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            nint hWndParent,
            nint hMenu,
            nint hInstance,
            nint lpParam
            );

        public delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        public static extern nint DefWindowProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(nint hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern nint GetModuleHandle(string strModuleName);
    }

}
