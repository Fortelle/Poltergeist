namespace Poltergeist.Automations.Utilities.Windows;

internal static class VirtualKeyExtension
{
    public static bool IsExtended(this VirtualKey key)
    {
        return key == VirtualKey.RMenu
            || key == VirtualKey.RControl
            || key == VirtualKey.Insert
            || key == VirtualKey.Delete
            || key == VirtualKey.Home
            || key == VirtualKey.End
            || key == VirtualKey.PageUp
            || key == VirtualKey.PageDown
            || key == VirtualKey.Left
            || key == VirtualKey.Up
            || key == VirtualKey.Right
            || key == VirtualKey.Down
            || key == VirtualKey.Divide
            || key == VirtualKey.NumLock
            || key == VirtualKey.Snapshot
            ;
        // numpad-enter: normal enter scancode with extended = true
    }

    public static bool IsModifier(this VirtualKey key)
    {
        return key == VirtualKey.Shift
            || key == VirtualKey.Menu
            || key == VirtualKey.Control
            || key == VirtualKey.LShift
            || key == VirtualKey.RShift
            || key == VirtualKey.LMenu
            || key == VirtualKey.RMenu
            || key == VirtualKey.LControl
            || key == VirtualKey.RControl
            ;
    }

}
