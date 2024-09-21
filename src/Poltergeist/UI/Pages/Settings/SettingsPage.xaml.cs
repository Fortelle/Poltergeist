using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Pages.Settings;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage(SettingsViewModel viewmodel)
    {
        ViewModel = viewmodel;

        InitializeComponent();
    }
}
