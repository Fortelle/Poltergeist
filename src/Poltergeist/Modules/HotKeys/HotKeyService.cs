using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Settings;

namespace Poltergeist.Modules.HotKeys;

public class HotKeyService : ServiceBase, IDisposable
{
    private HotKeyListener? Listener;

    private readonly List<HotKeyInformation> Informations = new();

    protected bool IsDisposed;

    private bool IsLoaded;

    public HotKeyService(AppEventService eventService)
    {
        eventService.Subscribe<AppWindowLoadedEvent>(OnAppWindowLoaded);
    }

    public void Add(HotKeyInformation info)
    {
        if (Informations.Any(x => x.Name == info.Name))
        {
            throw new ArgumentException($"Hot key '{info.Name}' is already registered.");
        }

        Informations.Add(info);

        if (info.SettingDefinition is not null)
        {
            PoltergeistApplication.GetService<AppSettingsService>().Settings.AddDefinition(info.SettingDefinition);
        }

        Logger.Trace($"Added hot key '{info.Name}'.", new
        {
            info.Name,
            SettingsKey = info.SettingDefinition?.Key
        });

        if (IsLoaded)
        {
            TryRegister(info);
        }
    }

    public void Change(string name, HotKey newHotKey)
    {
        if (Listener is null)
        {
            throw new InvalidOperationException($"{nameof(Listener)} should not be null.");
        }

        var info = Informations.FirstOrDefault(x => x.Name == name);
        if (info is null)
        {
            throw new KeyNotFoundException($"Hot key '{name}' is not registered.");
        }

        var oldHotKey = info.HotKey;
        if (oldHotKey is null)
        {
            return;
        }

        if (oldHotKey.Value == newHotKey)
        {
            return;
        }

        Listener.Unregister(oldHotKey.Value);
        Listener.Register(newHotKey);

        Logger.Trace($"Changed hot key '{info.Name}' from '{newHotKey}' to '{newHotKey}'.");
    }

    private void OnAppWindowLoaded(AppWindowLoadedEvent _)
    {
        Listener = new();
        Listener.HotkeyPressed += HotKeyPressed;

        foreach (var info in Informations)
        {
            TryRegister(info);
        }

        IsLoaded = true;
    }

    private void HotKeyPressed(HotKey hotkey)
    {
        Logger.Trace($"Hot key '{hotkey}' is pressed.");

        var info = Informations.FirstOrDefault(x => x.HotKey == hotkey);

        if (info is null)
        {
            Logger.Trace($"Could not find the information for the hot key '{hotkey}'.");
            return;
        }

        Task.Run(info.Callback);
    }

    private void TryRegister(HotKeyInformation info)
    {
        if (Listener is null)
        {
            throw new InvalidOperationException($"{nameof(Listener)} should not be null.");
        }

        var hotkey = info.HotKey;
        if (hotkey is null && info.SettingDefinition is not null)
        {
            hotkey = PoltergeistApplication.GetService<AppSettingsService>().Settings.Get(info.SettingDefinition);
            if (hotkey is null)
            {
                return;
            }
            info.HotKey = hotkey;
        }
        else
        {
            return;
        }

        Listener.Register(hotkey.Value);
        Logger.Trace($"Registered the hot key '{hotkey}'.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            if (Listener is not null)
            {
                Listener.HotkeyPressed -= HotKeyPressed;
                Listener.Dispose();
            }
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
