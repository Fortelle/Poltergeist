using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Tests.UnitTests.Components.Operations;

public class TestWindowHelper : IDisposable
{
    public string? ClassName { get; init; }
    public string? WindowName { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public bool TopMost { get; init; }

    public Bitmap? Background { get; init; }

    public nint Handle { get; private set; }

    public delegate void MouseEventHandler(MouseButtons Button, int X, int Y);

    public event MouseEventHandler? MouseUp;

    public event MouseEventHandler? MouseDown;

    private NativeMethods.WndProc? _wndProcKeepAlive;

    public void Build()
    {
        var className = ClassName;
        var windowName = WindowName;

        _wndProcKeepAlive = WndProcImpl;
        var wcx = new NativeMethods.WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.WNDCLASSEX>(),
            style = 0,
            lpfnWndProc = _wndProcKeepAlive,
            hInstance = NativeMethods.GetModuleHandle(null),
            hCursor = IntPtr.Zero,
            hbrBackground = NativeMethods.COLOR_WINDOW + 1,
            lpszClassName = className
        };

        var atom = NativeMethods.RegisterClassEx(ref wcx);
        Debug.Assert(atom != 0);

        var dwExStyle = (uint)0;
        if (TopMost)
        {
            dwExStyle |= NativeMethods.WS_EX_TOPMOST;
        }
        Handle = NativeMethods.CreateWindowEx(
            dwExStyle,
            className,
            windowName,
            NativeMethods.WS_POPUP,
            X, Y, Width, Height,
            nint.Zero,
            nint.Zero,
            wcx.hInstance,
            nint.Zero
            );
        Debug.Assert(Handle != nint.Zero);

        NativeMethods.ShowWindow(Handle, NativeMethods.SW_SHOW);
        NativeMethods.UpdateWindow(Handle);
    }

    public static void TranslateMessage()
    {
        NativeMethods.MSG msg;
        while (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
        {
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
        }
    }

    private nint WndProcImpl(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case NativeMethods.WM_LBUTTONUP:
                MouseUp?.Invoke(MouseButtons.Left, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_RBUTTONUP:
                MouseUp?.Invoke(MouseButtons.Right, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_MBUTTONUP:
                MouseUp?.Invoke(MouseButtons.Middle, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_XBUTTONUP:
                MouseUp?.Invoke(MouseButtons.XButton1, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_LBUTTONDOWN:
                MouseDown?.Invoke(MouseButtons.Left, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_RBUTTONDOWN:
                MouseDown?.Invoke(MouseButtons.Right, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_MBUTTONDOWN:
                MouseDown?.Invoke(MouseButtons.Middle, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_XBUTTONDOWN:
                MouseDown?.Invoke(MouseButtons.XButton1, NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam));
                return 0;
            case NativeMethods.WM_DESTROY:
                NativeMethods.PostQuitMessage(0);
                return 0;
            case NativeMethods.WM_PAINT:
                if (Background is not null)
                {
                    using var gra = Graphics.FromHwnd(Handle);
                    gra.DrawImage(Background, 0, 0, Width, Height);
                }
                return 0;
        }

        return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Close()
    {
        if (Handle != default)
        {
            WindowUtil.DestroyWindow(Handle);
        }

        Handle = default;
    }

    public void Dispose()
    {
        Close();
    }

    private static class NativeMethods
    {
        public const int CW_USEDEFAULT = unchecked((int)0x80000000);
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_VISIBLE = 0x10000000;

        public const int WM_DESTROY = 0x0002;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_PAINT = 0x000F;
        public const int HTCAPTION = 2;
        public const int VK_ESCAPE = 0x1B;

        public const int COLOR_WINDOW = 15;
        public const int WM_CREATE = 0x0001;
        public const int WM_NCCREATE = 0x0081;
        public const uint WS_EX_TOPMOST = 0x00000008;

        public const uint MK_LBUTTON = 0x0001;
        public const uint MK_RBUTTON = 0x0002;
        public const uint MK_SHIFT = 0x0004;
        public const uint MK_CONTROL = 0x0008;
        public const uint MK_MBUTTON = 0x0010;
        public const uint MK_XBUTTON1 = 0x0020;
        public const uint MK_XBUTTON2 = 0x0040;

        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_RBUTTONDBLCLK = 0x0206;
        public const uint WM_MBUTTONDOWN = 0x0207;
        public const uint WM_MBUTTONUP = 0x0208;
        public const uint WM_MBUTTONDBLCLK = 0x0209;
        public const uint WM_MOUSEWHEEL = 0x020A;
        public const uint WM_XBUTTONDOWN = 0x020B;
        public const uint WM_XBUTTONUP = 0x020C;
        public const uint WM_XBUTTONDBLCLK = 0x020D;
        public const uint WM_MOUSEHWHEEL = 0x020E;
        public const uint WM_MOUSELAST = 0x020E;

        public const uint WM_KEYDOWN = 0x100;
        public const uint WM_KEYUP = 0x0101;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)] public uint cbSize;
            [MarshalAs(UnmanagedType.U4)] public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)] public string? lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)] public string? lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public nint hwnd;
            public uint message;
            public nint wParam;
            public nint lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern nint CreateWindowEx(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string? lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string? lpWindowName,
            uint dwStyle,
            int X,
            int Y,
            int nWidth,
            int nHeight,
            nint hWndParent,
            nint hMenu,
            nint hInstance,
            nint lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(nint hWnd, int nCmdShow);

        public const int SW_SHOW = 5;

        [DllImport("user32.dll")] public static extern bool UpdateWindow(nint hWnd);
        [DllImport("user32.dll")] public static extern sbyte GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")] public static extern bool TranslateMessage([In] ref MSG lpMsg);
        [DllImport("user32.dll")] public static extern nint DispatchMessage([In] ref MSG lpMsg);
        [DllImport("user32.dll")] public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);
        [DllImport("user32.dll")] public static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll")] public static extern bool DestroyWindow(nint hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern nint LoadCursor(nint hInstance, nint lpCursorName);
        [DllImport("user32.dll")] public static extern int GetSystemMetrics(int nIndex);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] public static extern nint GetModuleHandle(string? lpModuleName);

        public static int GET_X_LPARAM(nint lParam)
        {
            return (int)(lParam) & 0xffff;
        }

        public static int GET_Y_LPARAM(nint lParam)
        {
            return (int)(lParam) >> 16;
        }
    }
}
