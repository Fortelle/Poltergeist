using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Poltergeist.Input.Windows;

public class WindowHelper
{
    private readonly IntPtr Handle;

    public WindowHelper(IntPtr hWnd)
    {
        Handle = hWnd;
    }

    public bool IsMinimized => NativeMethods.IsIconic(Handle);

    public string GetWindowName()
    {
        var stringLength = NativeMethods.GetWindowTextLength(Handle);
        if (stringLength > 0)
        {
            var sbString = new StringBuilder(stringLength + 1);
            NativeMethods.GetWindowText(Handle, sbString, sbString.Capacity);
            return sbString.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    public string GetClassName()
    {
        return GetClassName(Handle);
    }

    public Process GetProcess()
    {
        NativeMethods.GetWindowThreadProcessId(Handle, out var processId);
        var process = Process.GetProcessById((int)processId);
        return process;
    }

    public Rectangle? GetBounds()
    {
        return GetBounds(Handle);
    }

    public void BringToFront()
    {
        NativeMethods.SetForegroundWindow(Handle);
    }

    public void Minimize()
    {
        NativeMethods.ShowWindow(Handle, NativeMethods.SW_MINIMIZE);
    }

    public void Unminimize()
    {
        NativeMethods.ShowWindow(Handle, NativeMethods.SW_RESTORE);
    }

    public static string GetClassName(IntPtr hWnd)
    {
        var sbString = new StringBuilder(256);
        NativeMethods.GetClassName(hWnd, sbString, sbString.Capacity);
        return sbString.ToString();
    }

    public static Rectangle? GetBounds(IntPtr hWnd)
    {
        if (NativeMethods.DwmGetWindowAttribute(hWnd, NativeMethods.DwmWindowAttribute.ExtendedFrameBounds, out var dwmRect, Marshal.SizeOf(typeof(NativeMethods.RECT))) == 0)
        {
            return Rectangle.FromLTRB(dwmRect.Left, dwmRect.Top, dwmRect.Right, dwmRect.Bottom);
        }
        else if (NativeMethods.GetWindowRect(hWnd, out var gwrRect))
        {
            return Rectangle.FromLTRB(gwrRect.Left, gwrRect.Top, gwrRect.Right, gwrRect.Bottom);
        }
        else
        {
            return null;
        }
    }

    public static Bitmap Capture(IntPtr hWnd, Size size, uint flags = 0)
    {
        using var nativeGraphics = Graphics.FromHwnd(hWnd);
        var bmp = new Bitmap(size.Width, size.Height, nativeGraphics);
        using var memoryGraphics = Graphics.FromImage(bmp);
        var dc = memoryGraphics.GetHdc();
        var success = NativeMethods.PrintWindow(hWnd, dc, flags);
        memoryGraphics.ReleaseHdc(dc);
        return bmp;
    }

    private static class NativeMethods
    {
        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("USER32.DLL")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("USER32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_SHOWNORMAL = 1;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);

        public enum DwmWindowAttribute : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);












        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
        public const int PW_CLIENTONLY = 0x00000001;
        public const int PW_RENDERFULLCONTENT = 0x00000002;
    }
}
