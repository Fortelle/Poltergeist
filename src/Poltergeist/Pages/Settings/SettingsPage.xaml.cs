using Microsoft.UI.Xaml.Controls;

using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
