using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

public partial class WindowHelper
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
            var buffer = new char[stringLength + 1];
            NativeMethods.GetWindowText(Handle, buffer, buffer.Length);
            return new string(buffer);
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

    public void Maximize()
    {
        NativeMethods.ShowWindow(Handle, NativeMethods.SW_MAXIMIZE);
    }

    public void Minimize()
    {
        NativeMethods.ShowWindow(Handle, NativeMethods.SW_MINIMIZE);
    }
    
    public void Restore()
    {
        NativeMethods.ShowWindow(Handle, NativeMethods.SW_RESTORE);
    }

    public static string GetClassName(IntPtr hWnd)
    {
        var buffer = new char[256];
        NativeMethods.GetClassName(hWnd, buffer, buffer.Length);
        return new string(buffer).Trim('\0');
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

    // todo: try bitblt and dwm
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

    private static partial class NativeMethods
    {
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsIconic(IntPtr hWnd);

        [LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
        public static partial int GetWindowTextLength(IntPtr hWnd);

        [LibraryImport("user32.dll", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial int GetWindowText(IntPtr hWnd, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2)] char[] lpString, int nMaxCount);

        [LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial int GetClassName(IntPtr hWnd, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2)] char[] lpClassName, int nMaxCount);

        [LibraryImport("user32.dll")]
        public static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_SHOWNORMAL = 1;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        
        [LibraryImport("dwmapi.dll")]
        public static partial int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);

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

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        public const int PW_CLIENTONLY = 0x00000001;
        public const int PW_RENDERFULLCONTENT = 0x00000002;
    }
}
