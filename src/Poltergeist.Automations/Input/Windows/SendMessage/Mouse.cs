using System.Drawing;

namespace Poltergeist.Input.Windows;

public partial class SendMessageHelper
{
    public SendMessageHelper MouseButtonUp(uint x, uint y, MouseButtons button)
    {
        DoMouseButton(x, y, button, true);
        return this;
    }

    public SendMessageHelper MouseButtonDown(uint x, uint y, MouseButtons button)
    {
        DoMouseButton(x, y, button, false);
        return this;
    }

    private void DoMouseButton(uint x, uint y, MouseButtons button, bool isUp)
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

        uint wParam = button switch
        {
            MouseButtons.Left => NativeMethods.MK_LBUTTON,
            MouseButtons.Right => NativeMethods.MK_RBUTTON,
            MouseButtons.Middle => NativeMethods.MK_MBUTTON,
            MouseButtons.XButton1 => NativeMethods.MK_XBUTTON1,
            MouseButtons.XButton2 => NativeMethods.MK_XBUTTON2,
            _ => 0,
        };

        var lParam = (x & 0xFFFF) | (y << 16);

        NativeMethods.SendMessage(Hwnd, wm, (nint)wParam, (nint)lParam);
    }

}
