using System.Threading.Tasks;
using Poltergeist.Input.Windows;

namespace Poltergeist.Services;

public class ActivationService
{
    public ActivationService()
    {
    }

    public async Task ActivateAsync(object activationArgs = null)
    {
        await InitializeAsync();

        await HandleActivationAsync(activationArgs);

        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
    }

    private async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        var plugin = App.GetService<PluginService>();
        await plugin.Load();

        var macroManager = App.GetService<MacroManager>();
        macroManager.LoadGlobalOptions();
        macroManager.LoadRecentMacros();

        var hotkey = App.GetService<HotKeyService>();
        hotkey.Register(new (VirtualKey.F8), macroManager.Toggle);

        await Task.CompletedTask;
    }

}
