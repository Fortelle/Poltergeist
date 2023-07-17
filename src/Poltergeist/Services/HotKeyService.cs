using Poltergeist.Input.Windows;

namespace Poltergeist.Services;

public class HotKeyService : IDisposable
{
    private readonly HotKeyListener Listener;

    private Dictionary<HotKey, Action> Actions
    {
        get; set;
    }

    public HotKeyService()
    {
        Listener = new();

        Actions = new();
        Listener.HotkeyPressed += HotKeyPressed;
    }

    public bool Register(HotKey hotkey, Action action)
    {
        var result = Listener.Register(hotkey);
        if (result)
        {
            Actions.Add(hotkey, action);
        }
        return result;
    }

    private void HotKeyPressed(HotKey hotkey)
    {
        if(Actions.TryGetValue(hotkey, out var action))
        {
            action.Invoke();
        }
    }

    public void Dispose()
    {
        Listener.HotkeyPressed -= HotKeyPressed;
        Listener.Dispose();
    }

    public bool Unregister(HotKey hotkey)
    {
        return Listener.Unregister(hotkey);
    }
}
