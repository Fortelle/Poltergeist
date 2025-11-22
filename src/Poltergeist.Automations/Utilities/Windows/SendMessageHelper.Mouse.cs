namespace Poltergeist.Automations.Utilities.Windows;

public partial class SendMessageHelper
{
    public SendMessageHelper MouseButtonUp(int x, int y, MouseButtons button, KeyModifiers modifier)
    {
        DoMouseButton(x, y, button, true, modifier);
        return this;
    }

    public SendMessageHelper MouseButtonDown(int x, int y, MouseButtons button, KeyModifiers modifier)
    {
        DoMouseButton(x, y, button, false, modifier);
        return this;
    }

    public SendMessageHelper MouseDoubleClick(int x, int y, MouseButtons button, KeyModifiers modifier)
    {
        DoMouseDoubleClick(x, y, button, false, modifier);
        return this;
    }

    private void DoMouseButton(int x, int y, MouseButtons button, bool isUp, KeyModifiers modifier)
    {
        var wm = button switch
        {
            MouseButtons.Left when isUp => NativeMethods.WM_LBUTTONUP,
            MouseButtons.Right when isUp => NativeMethods.WM_RBUTTONUP,
            MouseButtons.Middle when isUp => NativeMethods.WM_MBUTTONUP,
            MouseButtons.XButton1 when isUp => NativeMethods.WM_XBUTTONUP,
            MouseButtons.XButton2 when isUp => NativeMethods.WM_XBUTTONUP,

            MouseButtons.Left => NativeMethods.WM_LBUTTONDOWN,
            MouseButtons.Right => NativeMethods.WM_RBUTTONDOWN,
            MouseButtons.Middle => NativeMethods.WM_MBUTTONDOWN,
            MouseButtons.XButton1 => NativeMethods.WM_XBUTTONDOWN,
            MouseButtons.XButton2 => NativeMethods.WM_XBUTTONDOWN,

            _ => throw new NotImplementedException(),
        };

        var wParam = MakeWParam(button, modifier);
        var lParam = MakeLParam(x, y);

        NativeMethods.SendMessage(Hwnd, wm, wParam, lParam);

        Logger?.Trace($"SendMessage(0x{Hwnd:X8}, 0x{wm:X8}, 0x{wParam:X8}, 0x{lParam:X8})");
    }

    private void DoMouseDoubleClick(int x, int y, MouseButtons button, bool isUp, KeyModifiers modifier)
    {
        var wm = button switch
        {

            MouseButtons.Left => NativeMethods.WM_LBUTTONDBLCLK,
            MouseButtons.Right => NativeMethods.WM_RBUTTONDBLCLK,
            MouseButtons.Middle => NativeMethods.WM_MBUTTONDBLCLK,
            MouseButtons.XButton1 => NativeMethods.WM_XBUTTONDBLCLK,
            MouseButtons.XButton2 => NativeMethods.WM_XBUTTONDBLCLK,

            _ => throw new NotImplementedException(),
        };

        var wParam = MakeWParam(button, modifier);
        var lParam = MakeLParam(x, y);

        NativeMethods.SendMessage(Hwnd, wm, wParam, lParam);

        Logger?.Trace($"SendMessage(0x{Hwnd:X8}, 0x{wm:X8}, 0x{wParam:X8}, 0x{lParam:X8})");
    }

    private static nint MakeWParam(MouseButtons button, KeyModifiers modifier)
    {
        uint wParam = button switch
        {
            MouseButtons.Left => NativeMethods.MK_LBUTTON,
            MouseButtons.Right => NativeMethods.MK_RBUTTON,
            MouseButtons.Middle => NativeMethods.MK_MBUTTON,
            MouseButtons.XButton1 => NativeMethods.MK_XBUTTON1,
            MouseButtons.XButton2 => NativeMethods.MK_XBUTTON2,
            _ => 0,
        };
        wParam |= modifier switch
        {
            KeyModifiers.Shift => NativeMethods.MK_SHIFT,
            KeyModifiers.Control => NativeMethods.MK_CONTROL,
            _ => 0,
        };
        return (nint)wParam;
    }

    private static nint MakeLParam(int x, int y)
    {
        var lParam = (x & 0xFFFF) | (y << 16);
        return lParam;
    }

    // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove
    public SendMessageHelper MouseMove(int x, int y, MouseButtons button = MouseButtons.None, KeyModifiers modifier = KeyModifiers.None)
    {
        var wParam = MakeWParam(button, modifier);
        var lParam = MakeLParam(x, y);

        NativeMethods.SendMessage(Hwnd, NativeMethods.WM_MOUSEMOVE, wParam, lParam);

        Logger?.Trace($"SendMessage(0x{Hwnd:X8}, 0x{NativeMethods.WM_MOUSEMOVE:X8}, 0x{wParam:X8}, 0x{lParam:X8})");

        return this;
    }

    // https://learn.microsoft.com/zh-cn/windows/win32/inputdev/wm-mousewheel
    public SendMessageHelper MouseWheel(int x, int y, int detents, MouseButtons button = MouseButtons.None, KeyModifiers modifier = KeyModifiers.None)
    {
        var distance = NativeMethods.WHEEL_DELTA * detents;

        var wParam = MakeWParam(button, modifier) | (distance << 16);
        var lParam = MakeLParam(x, y);

        NativeMethods.SendMessage(Hwnd, NativeMethods.WM_MOUSEWHEEL, wParam, lParam);

        Logger?.Trace($"SendMessage(0x{Hwnd:X8}, 0x{NativeMethods.WM_MOUSEWHEEL:X8}, 0x{wParam:X8}, 0x{lParam:X8})");

        return this;
    }

    // https://learn.microsoft.com/zh-cn/windows/win32/inputdev/wm-mousehwheel
    public SendMessageHelper MouseHWheel(int x, int y, int detents, MouseButtons button = MouseButtons.None, KeyModifiers modifier = KeyModifiers.None)
    {
        var distance = NativeMethods.WHEEL_DELTA * detents;

        var wParam = MakeWParam(button, modifier) | (distance << 16);
        var lParam = MakeLParam(x, y);

        NativeMethods.SendMessage(Hwnd, NativeMethods.WM_MOUSEHWHEEL, wParam, lParam);

        Logger?.Trace($"SendMessage(0x{Hwnd:X8}, 0x{NativeMethods.WM_MOUSEHWHEEL:X8}, 0x{wParam:X8}, 0x{lParam:X8})");

        return this;
    }
}
