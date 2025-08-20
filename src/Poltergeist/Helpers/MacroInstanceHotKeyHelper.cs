using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.HotKeys;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Helpers;

public static class MacroInstanceHotKeyHelper
{
    public static void Inject()
    {
        var eventService = PoltergeistApplication.GetService<AppEventService>();
        eventService.Subscribe<AppContentLoadingEvent>(OnAppContentLoading);
        eventService.Subscribe<MacroInstancePropertyChangedEvent>(OnMacroInstancePropertyChanged);
        eventService.Subscribe<MacroInstanceCollectionChangedEvent>(OnMacroInstanceCollectionChanged);
    }

    private static void OnAppContentLoading(AppContentLoadingEvent _)
    {
        var instanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();

        foreach (var instance in instanceManager.GetInstances())
        {
            RegisterInstance(instance);
        }
    }

    private static void RegisterInstance(MacroInstance instance)
    {
        if (!instance.IsValid)
        {
            return;
        }
        if (instance.Properties is null)
        {
            return;
        }
        if (instance.Properties.HotKey == HotKey.Empty)
        {
            return;
        }

        try
        {
            PoltergeistApplication.GetService<HotKeyService>().Add(new($"macroinstance_{instance.InstanceId}", instance.Properties.HotKey, OnHotKeyPressed));
        }
        catch
        {
        }
    }

    private static void UnregisterInstance(MacroInstance instance)
    {
        if (instance.Properties is null)
        {
            return;
        }
        if (instance.Properties.HotKey == HotKey.Empty)
        {
            return;
        }

        try
        {
            PoltergeistApplication.GetService<HotKeyService>().Remove($"macroinstance_{instance.InstanceId}");
        }
        catch
        {
        }
    }

    private static void OnMacroInstanceCollectionChanged(MacroInstanceCollectionChangedEvent e)
    {
        if (e.NewItems?.Length > 0)
        {
            foreach (var instance in e.NewItems)
            {
                RegisterInstance(instance);
            }
        }
        if (e.OldItems?.Length > 0)
        {
            foreach (var instance in e.OldItems)
            {
                UnregisterInstance(instance);
            }
        }
    }

    private static void OnMacroInstancePropertyChanged(MacroInstancePropertyChangedEvent e)
    {
        var instance = e.Instance;
        if (!instance.IsValid)
        {
            return;
        }
        if (instance.Properties is null)
        {
            return;
        }

        var hotkeyName = $"macroinstance_{instance.InstanceId}";
        var hotkeyService = PoltergeistApplication.GetService<HotKeyService>();
        var hotkeyInfo = hotkeyService.Get(hotkeyName);
        if (hotkeyInfo is null)
        {
            return;
        }
        if (hotkeyInfo.HotKey == instance.Properties.HotKey)
        {
            return;
        }

        if (instance.Properties.HotKey == HotKey.Empty)
        {
            hotkeyService.Remove(hotkeyName);
        }
        else
        {
            hotkeyService.Change(hotkeyName, instance.Properties.HotKey);
        }
    }

    private static void OnHotKeyPressed(HotKey hotkey)
    {
        var instance = PoltergeistApplication.GetService<MacroInstanceManager>().GetInstances().FirstOrDefault(x => x.Properties?.HotKey == hotkey);
        if (instance is null)
        {
            return;
        }

        PoltergeistApplication.TryEnqueue(() =>
        {
            if (PoltergeistApplication.GetService<MacroManager>().OpenPage(instance, out var viewmodel))
            {
                viewmodel.Start();
            }
        });
    }
}