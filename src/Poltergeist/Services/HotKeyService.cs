using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Services;

public class HotKeyService : IDisposable
{
    private readonly HotKeyListener Listener = new();
    private readonly Dictionary<HotKey, Action> Actions = new();

    protected bool IsDisposed;

    public HotKeyService()
    {
        Listener.HotkeyPressed += HotKeyPressed;
    }

    public bool Register(HotKey hotkey, Action action)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (Actions.ContainsKey(hotkey))
        {
            return false;
        }

        var result = Listener.Register(hotkey);

        if (!result)
        {
            return false;
        }

        Actions.Add(hotkey, action);

        return result;
    }

    public bool Unregister(HotKey hotkey)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return Listener.Unregister(hotkey);
    }

    private void HotKeyPressed(HotKey hotkey)
    {
        if(Actions.TryGetValue(hotkey, out var action))
        {
            action.Invoke();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            Listener.HotkeyPressed -= HotKeyPressed;
            Listener.Dispose();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
