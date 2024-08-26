using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities.Windows;

public partial class SendInputHelper
{
    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint MapVirtualKey(uint uCode, MapVirtualKeyMapTypes uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int OemKeyScan(short wAsciiVal);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT point);

        [Flags]
        public enum MouseEventFlags : int
        {
            None = 0x0000,
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            HWheel = 0x1000,
            MoveNoCoalesce = 0x2000,
            VirtualDesk = 0x4000,
            Absolute = 0x8000,
        }

        public enum MapVirtualKeyMapTypes : uint
        {
            MAPVK_VK_TO_VSC = 0x00,
            MAPVK_VSC_TO_VK = 0x01,
            MAPVK_VK_TO_CHAR = 0x02,
            MAPVK_VSC_TO_VK_EX = 0x03,
            MAPVK_VK_TO_VSC_EX = 0x04
        }

        [Flags]
        public enum KeyEventFlags : int
        {
            None = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            ScanCode = 0x0008,
        }

        public enum InputType : int
        {
            Mouse,
            Keyboard,
            Hardware,
        }

        /// <remarks>
        /// https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-input
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public InputType type;
            public INPUTUNION inputUnion;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        /// <remarks>
        /// https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-mouseinput
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public MouseEventFlags dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        /// <remarks>
        /// https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-keybdinput
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public KeyEventFlags dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        /// <remarks>
        /// https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-hardwareinput
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }



        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        public const int KEY_TOGGLED = 0x1;
        public const int KEY_PRESSED = 0x8000;
    }
}