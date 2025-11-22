using System.Diagnostics;
using System.Drawing;

namespace Poltergeist.Automations.Utilities.Windows;

public class WindowHelper
{
    private readonly nint Handle;

    public WindowHelper(nint hWnd)
    {
        Handle = hWnd;
    }

    public bool IsMinimized => WindowUtil.IsMinimized(Handle);

    public string GetWindowName()
    {
        return WindowUtil.GetWindowName(Handle);
    }

    public string GetClassName()
    {
        return WindowUtil.GetClassName(Handle);
    }

    public Process GetProcess()
    {
        return WindowUtil.GetProcess(Handle);
    }

    public Rectangle? GetBounds()
    {
        return WindowUtil.GetBounds(Handle);
    }

    public void BringToFront()
    {
        WindowUtil.BringToFront(Handle);
    }

    public void CenterToScreen()
    {
        WindowUtil.CenterToScreen(Handle);
    }

    public void Maximize()
    {
        WindowUtil.Maximize(Handle);
    }

    public void Minimize()
    {
        WindowUtil.Minimize(Handle);
    }
    
    public void Restore()
    {
        WindowUtil.Restore(Handle);
    }

    public void Close()
    {
        WindowUtil.DestroyWindow(Handle);
    }

    public Bitmap Capture()
    {
        return WindowUtil.Capture(Handle);
    }
}
