using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

public static partial class WindowUtil
{
    public static bool IsMinimized(nint hWnd) => NativeMethods.IsIconic(hWnd);

    public static bool IsForeground(nint hWnd) => NativeMethods.GetForegroundWindow() == hWnd;

    public static string GetWindowName(nint hWnd)
    {
        var stringLength = NativeMethods.GetWindowTextLength(hWnd);
        if (stringLength > 0)
        {
            var buffer = new char[stringLength + 1];
            NativeMethods.GetWindowText(hWnd, buffer, buffer.Length);
            return new string(buffer);
        }
        else
        {
            return string.Empty;
        }
    }

    public static string GetClassName(nint hWnd)
    {
        var buffer = new char[256];
        NativeMethods.GetClassName(hWnd, buffer, buffer.Length);
        return new string(buffer).Trim('\0');
    }

    public static Process GetProcess(nint hWnd)
    {
        NativeMethods.GetWindowThreadProcessId(hWnd, out var processId);
        var process = Process.GetProcessById((int)processId);
        return process;
    }

    public static Rectangle? GetBounds(nint hWnd)
    {
        if (NativeMethods.DwmGetWindowAttribute(hWnd, NativeMethods.DwmWindowAttribute.ExtendedFrameBounds, out var dwmRect, Marshal.SizeOf<NativeMethods.RECT>()) == 0)
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

    public static void BringToFront(nint hWnd)
    {
        NativeMethods.SetForegroundWindow(hWnd);
    }

    public static void CenterToScreen(nint hWnd)
    {
        if (!NativeMethods.GetWindowRect(hWnd, out var windowRect))
        {
            throw new Exception();
        }

        var systemWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        var systemHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);

        var newX = (systemWidth - (windowRect.Right - windowRect.Left)) / 2;
        var newY = (systemHeight - (windowRect.Bottom - windowRect.Top)) / 2;
        NativeMethods.SetWindowPos(
            hWnd,
            default,
            newX,
            newY,
            -1,
            -1,
            NativeMethods.SetWindowPosFlags.IgnoreResize | NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.FrameChanged | NativeMethods.SetWindowPosFlags.DoNotActivate
            );
    }

    public static void Maximize(nint hWnd)
    {
        NativeMethods.ShowWindow(hWnd, NativeMethods.ShowWindowCommands.ShowMaximized);
    }

    public static void Minimize(nint hWnd)
    {
        NativeMethods.ShowWindow(hWnd, NativeMethods.ShowWindowCommands.Minimize);
    }
    
    public static void Restore(nint hWnd)
    {
        NativeMethods.ShowWindow(hWnd, NativeMethods.ShowWindowCommands.Restore);
    }

    public static void DestroyWindow(nint hWnd)
    {
        NativeMethods.DestroyWindow(hWnd);
    }

    public static Bitmap Capture(nint hWnd, Size size)
    {
        using var nativeGraphics = Graphics.FromHwnd(hWnd);
        var bmp = new Bitmap(size.Width, size.Height, nativeGraphics);
        using var memoryGraphics = Graphics.FromImage(bmp);
        var dc = memoryGraphics.GetHdc();
        var flags = 2u;
        var success = NativeMethods.PrintWindow(hWnd, dc, flags);
        memoryGraphics.ReleaseHdc(dc);
        return bmp;
    }

    public static Bitmap Capture(nint hWnd)
    {
        var rect = GetBounds(hWnd);
        Debug.Assert(rect.HasValue);
        return Capture(hWnd, rect.Value.Size);
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
        public static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        public enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [LibraryImport("user32.dll")]
        public static partial int GetSystemMetrics(int nIndex);

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;


        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            IgnoreResize = 0x0001,
            IgnoreMove = 0x0002,
            IgnoreZOrder = 0x0004,
            DoNotRedraw = 0x0008,
            DoNotActivate = 0x0010,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            ShowWindow = 0x0040,
            HideWindow = 0x0080,
            DoNotCopyBits = 0x0100,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotReposition = DoNotChangeOwnerZOrder,
            DoNotSendChangingEvent = 0x0400,
            DeferErase = 0x2000,
            AsynchronousWindowPosition = 0x4000,
        }

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

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DestroyWindow(IntPtr hWnd);
    }
}
