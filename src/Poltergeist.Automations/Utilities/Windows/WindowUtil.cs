using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

public static partial class WindowUtil
{
    public static bool IsMinimized(nint hWnd) => NativeMethods.IsIconic(hWnd);

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

public unsafe partial class BitBltHelper : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public IntPtr Hwnd { get; }

    //public IntPtr HdcSrc { get; private set; }
    public IntPtr HdcDest { get; private set; }
    public IntPtr HBitmap { get; private set; }
    public IntPtr HOldBmp { get; private set; }

    private byte* pixelPtr;   // 指向位图的原始像素

    private int strideBytes;  // 每行的字节数

    public BitBltHelper(IntPtr hwnd)
    {
        Hwnd = hwnd;

        NativeMethods.RECT rect;
        NativeMethods.GetWindowRect(hwnd, out rect);
        Width = rect.Right - rect.Left;
        Height = rect.Bottom - rect.Top;

        var hdcSrc = NativeMethods.GetWindowDC(Hwnd);
        HdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
        HBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, Width, Height);
        HOldBmp = NativeMethods.SelectObject(HdcDest, HBitmap);

        // 获取 HBITMAP 信息
        NativeMethods.BITMAP bmpInfo = new NativeMethods.BITMAP();
        NativeMethods.GetObject(HBitmap, Marshal.SizeOf(bmpInfo), ref bmpInfo);

        pixelPtr = (byte*)bmpInfo.bmBits;          // 原始像素指针
        strideBytes = bmpInfo.bmWidthBytes;        // 每行字节数（通常=Width*4）

        _ = NativeMethods.ReleaseDC(hwnd, hdcSrc);
    }

    //public void RefreshHdc()
    //{
    //    HdcSrc = NativeMethods.GetWindowDC(Hwnd);
    //}

    public void Capture()
    {
        var hdcSrc = NativeMethods.GetWindowDC(Hwnd);
        NativeMethods.BitBlt(HdcDest, 0, 0, Width, Height, hdcSrc, 0, 0, CopyPixelOperation.SourceCopy);
        _ = NativeMethods.ReleaseDC(Hwnd, hdcSrc);
    }

    public Span<byte> GetSpan()
    {
        return new Span<byte>(pixelPtr, strideBytes * Height);
    }

    public Bitmap ToBitmap()
    {
        return Image.FromHbitmap(HBitmap);
    }

    public void Dispose()
    {
        NativeMethods.SelectObject(HdcDest, HOldBmp);
        NativeMethods.DeleteObject(HBitmap);
        NativeMethods.DeleteDC(HdcDest);
    }


    public static Bitmap Capture(nint hwnd)
    {
        NativeMethods.RECT rect;
        NativeMethods.GetWindowRect(hwnd, out rect);
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        return Capture(hwnd, new Rectangle(0, 0, width, height));
    }

    public static Bitmap Capture(nint hwnd, Size size)
    {
        return Capture(hwnd, new Rectangle(default, size));
    }

    public static Bitmap Capture(nint hwnd, Rectangle rect)
    {
        var hdcSrc = NativeMethods.GetWindowDC(hwnd);
        var hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
        var hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
        var hOldBmp = NativeMethods.SelectObject(hdcDest, hBitmap);
        NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, CopyPixelOperation.SourceCopy);
        _ = NativeMethods.ReleaseDC(hwnd, hdcSrc);

        var bmp = Image.FromHbitmap(hBitmap);

        NativeMethods.SelectObject(hdcDest, hOldBmp);
        NativeMethods.DeleteObject(hBitmap);
        NativeMethods.DeleteDC(hdcDest);

        return bmp;
    }

    public static Bitmap[] Capture(nint hwnd, Rectangle[] rectangles)
    {
        var hdcSrc = NativeMethods.GetWindowDC(hwnd);
        var hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
        var list = new List<Bitmap>();
        foreach (var rect in rectangles)
        {
            var hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
            var hOldBmp = NativeMethods.SelectObject(hdcDest, hBitmap);
            NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, CopyPixelOperation.SourceCopy);

            var bmp = Image.FromHbitmap(hBitmap);
            list.Add(bmp);

            NativeMethods.SelectObject(hdcDest, hOldBmp);
            NativeMethods.DeleteObject(hBitmap);
        }

        _ = NativeMethods.ReleaseDC(hwnd, hdcSrc);
        NativeMethods.DeleteDC(hdcDest);

        return [.. list];
    }

    private static partial class NativeMethods
    {
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(
            IntPtr hdcDest, int xDest, int yDest, int width, int height,
            IntPtr hdcSrc, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);


        [DllImport("gdi32.dll")]
        public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, ref BITMAP lpvObject);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;   // 原始 pixel 指针
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
