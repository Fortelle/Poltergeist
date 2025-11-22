using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

public partial class SendMessageHelper
{
    private static partial class NativeMethods
    {
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

        public const int WHEEL_DELTA = 120;

        [LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
        public static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
        public static partial IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

    }
}