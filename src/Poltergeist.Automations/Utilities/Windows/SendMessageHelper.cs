namespace Poltergeist.Automations.Utilities.Windows;

public partial class SendMessageHelper
{
    public IntPtr Hwnd;

    public SendMessageHelper(IntPtr hwnd)
    {
        Hwnd = hwnd;
    }

}