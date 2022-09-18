using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Poltergeist.Input.Windows;

public class HotKeyListener : IDisposable
{
    public delegate void HotkeyPressHandler(HotKey hotkey);
    public event HotkeyPressHandler HotkeyPressed;

    private readonly IntPtr Hwnd;
    private readonly NativeMethods.WndProcDelegate wndProc;
    private int _idCounter;
    private readonly Dictionary<HotKey, int> HotKeyList;

    public HotKeyListener()
    {
        HotKeyList = new();

        var hInstance = IntPtr.Zero;
        wndProc = new NativeMethods.WndProcDelegate(WindowProc);

        var wndClassEx = new NativeMethods.WNDCLASSEX
        {
            cbSize = Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX)),
            style = 0,
            cbClsExtra = 0,
            cbWndExtra = 0,
            hbrBackground = IntPtr.Zero,
            hCursor = IntPtr.Zero,
            hIcon = IntPtr.Zero,
            hIconSm = IntPtr.Zero,
            lpszClassName = "HotKeyClass",
            lpszMenuName = string.Empty,
            hInstance = hInstance,
            lpfnWndProc = wndProc,
        };
        var atom = NativeMethods.RegisterClassEx(ref wndClassEx);

        if (atom == 0)
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }

        Hwnd = NativeMethods.CreateWindowEx(
           NativeMethods.WS_EX_NOACTIVATE,
           (ushort)atom,
           "HotKeyWindow",
           0,
           0,
           0,
           0,
           0,
           IntPtr.Zero,
           IntPtr.Zero,
           hInstance,
           IntPtr.Zero
       );

        if (Hwnd == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }
    }

    public void Dispose()
    {
        NativeMethods.UnregisterClass("HotKeyClass", Hwnd);
        foreach (var hotkey in HotKeyList.Keys)
        {
            Unregister(hotkey);
        }
    }

    public bool Register(HotKey hotkey)
    {
        if (HotKeyList.ContainsKey(hotkey))
        {
            return false;
        }

        var id = Interlocked.Increment(ref _idCounter);

        var isSucceeded = NativeMethods.RegisterHotKey(Hwnd, id, (uint)hotkey.Modifiers, (uint)hotkey.KeyCode);

        if (isSucceeded)
        {
            HotKeyList.Add(hotkey, id);
        }

        return isSucceeded;
    }

    public bool Unregister(HotKey hotkey)
    {
        if (!HotKeyList.TryGetValue(hotkey, out var id))
        {
            return false;
        }

        var isSucceeded = NativeMethods.UnregisterHotKey(Hwnd, id);

        if (isSucceeded)
        {
            HotKeyList.Remove(hotkey);
        }

        return isSucceeded;
    }

    private IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            var id = (int)wParam;
            var key = lParam;
            var hotkey = HotKeyList.First(x => x.Value == id).Key;
            HotkeyPressed?.Invoke(hotkey);
            return IntPtr.Zero;
        }
        else
        {
            return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }

    private static class NativeMethods
    {
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(uint dwExStyle, ushort regClassResult,
           string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight,
           IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr pvParam);

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    }
}
