namespace Poltergeist.Input.Windows;

public partial class SendMessageHelper
{
    public IntPtr Hwnd;

    public SendMessageHelper(IntPtr hwnd)
    {
        Hwnd = hwnd;
    }

}