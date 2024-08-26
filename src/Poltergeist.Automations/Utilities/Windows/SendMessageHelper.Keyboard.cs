namespace Poltergeist.Automations.Utilities.Windows;

// Warning: SendMessage and PostMessage do not support modifier keys.
public partial class SendMessageHelper
{
    public SendMessageHelper KeyDown(VirtualKey key)
    {
        uint repeatCount = 0; // 0-15, todo
        uint scanCode = (uint)key; // 16-23
        uint extended = (uint)(key.IsModifier() ? 1 : 0); // 24
        // 25-28, reversed
        uint context = 0;  // 29, always 0
        uint previousState = 0; // 30, todo
        uint transition = 0; // 31, always 0

        var lParam = repeatCount
            | (scanCode << 16)
            | (extended << 24)
            | (context << 29)
            | (previousState << 30)
            | (transition << 31);

        NativeMethods.PostMessage(Hwnd, NativeMethods.WM_KEYDOWN, (nint)key, (nint)lParam);
        return this;
    }

    public SendMessageHelper KeyUp(VirtualKey key)
    {
        uint repeatCount = 1; // 0-15, always 1
        uint scanCode = (uint)key; // 16-23
        uint extended = (uint)(key.IsModifier() ? 1 : 0); // 24
        // 25-28, reversed
        uint context = 0;  // 29, always 0
        uint previousState = 1; // 30, always 1
        uint transition = 1; // 31, always 1

        var lParam = repeatCount
            | (scanCode << 16)
            | (extended << 24)
            | (context << 29)
            | (previousState << 30)
            | (transition << 31);

        NativeMethods.PostMessage(Hwnd, NativeMethods.WM_KEYUP, (nint)key, (nint)lParam);
        return this;
    }

}
