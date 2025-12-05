using System.Drawing;
using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

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
