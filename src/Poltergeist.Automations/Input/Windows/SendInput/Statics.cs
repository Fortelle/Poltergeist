using System.Drawing;

namespace Poltergeist.Input.Windows;

public partial class SendInputHelper
{

    public static void MouseUp(MouseButtons button, int data = 0)
    {
        new SendInputHelper()
            .AddMouseUp(button, data)
            .Execute();
    }

    public static void MouseDown(MouseButtons button, int data = 0)
    {
        new SendInputHelper()
            .AddMouseDown(button, data)
            .Execute();
    }

    public static void MouseWheel(int detents)
    {
        new SendInputHelper()
            .AddMouseWheel(detents)
            .Execute();
    }

    public static void MouseHWheel(int detents)
    {
        new SendInputHelper()
            .AddMouseHWheel(detents)
            .Execute();
    }

    public static Point Cursor
    {
        get
        {
            NativeMethods.GetCursorPos(out var p);
            return new Point(p.X, p.Y);
        }
        set
        {
            NativeMethods.SetCursorPos(value.X, value.Y);
        }
    }

    public static bool IsKeyToggled(VirtualKey key)
    {
        return (NativeMethods.GetKeyState((int)key) & NativeMethods.KEY_TOGGLED) != 0;
    }

    public static void Send(string text, int interval = 0)
    {
        foreach (var c in text)
        {
            new SendInputHelper()
                .AddUnicodeDown(c)
                .AddUnicodeUp(c)
                .Execute();
            if(interval > 0)
            {
                Thread.Sleep(interval);
            }
        }
    }

    public static void KeyPress(params VirtualKey[] keys)
    {
        var sendInput = new SendInputHelper();
        for(var i = 0; i < keys.Length; i++)
        {
            sendInput.AddScancodeDown(keys[i]);
        }
        for (var i = keys.Length-1; i >=0; i--)
        {
            sendInput.AddScancodeUp(keys[i]);
        };
        sendInput.Execute();
    }

    public static bool IsCapsLockToggled => IsKeyToggled(VirtualKey.Capital);
    public static bool IsNumLockToggled => IsKeyToggled(VirtualKey.NumLock);
    public static bool IsScrollToggled => IsKeyToggled(VirtualKey.Scroll);

}
