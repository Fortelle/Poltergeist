using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Modules.HotKeys;
using Poltergeist.Modules.Navigation;
using Poltergeist.UI.Pages.Macros;

namespace Poltergeist.Helpers;

public static class MacroStartKeyHelper
{
    public static HotKeyInformation HotKeyInformation = new("macrostartkey")
    {
        SettingDefinition = new("macro.startkey")
        {
            Category = ResourceHelper.Localize("Poltergeist/Resources/AppSettings_Macro"),
            DisplayLabel = ResourceHelper.Localize("Poltergeist/Resources/AppSettings_Macro_StartMacroHotKey"),
        },

        Callback = _ =>
        {
            App.TryEnqueue(() =>
            {
                RunMacro(LaunchReason.Manual, true);
            });
        },
    };

    public static void OnSettingChanged(HotKey newKey)
    {
        var hotkeyService = PoltergeistApplication.GetService<HotKeyService>();
        hotkeyService.Change(HotKeyInformation.SettingDefinition!.Key, newKey);
    }

    private static void RunMacro(LaunchReason reason, bool toggle)
    {
        var navigationService = PoltergeistApplication.GetService<NavigationService>();
        if (navigationService.TabView!.SelectedItem is not TabViewItem tabViewItem)
        {
            return;
        }

        if (tabViewItem.Content is not MacroPage macroPage)
        {
            return;
        }

        if (macroPage.ViewModel.IsRunning)
        {
            if (toggle)
            {
                macroPage.ViewModel.Stop();
            }
            else
            {
                PoltergeistApplication.ShowTeachingTip(PoltergeistApplication.Localize($"Poltergeist/Macro/MacroAlreadyRunning", macroPage.ViewModel.Instance.Title));
                return;
            }
        }
        else
        {
            macroPage.ViewModel.Start(null, reason);
        }
    }

}
