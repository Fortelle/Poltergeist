using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Interactions;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.UI.Pages.Home;

public sealed partial class MacroBrowser : UserControl
{
    public MacroBrowserViewModel ViewModel { get; }

    public MacroBrowser()
    {
        ViewModel = new();

        InitializeComponent();
    }

    private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is not MacroShell shell)
        {
            return;
        }

        var nav = App.GetService<INavigationService>();
        nav.NavigateTo("macro:" + shell.ShellKey);
    }

    private void ListViewHeader_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var column = Grid.GetColumn((FrameworkElement)sender);
        ViewModel.Sort(column);
    }

    private void OpenMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroShell shell)
        {
            return;
        }

        var nav = App.GetService<INavigationService>();
        nav.NavigateTo("macro:" + shell.ShellKey);
    }

    private async void RenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroShell shell)
        {
            return;
        }

        if (shell.Template?.IsSingleton != false)
        {
            return;
        }

        var dialogModel = new InputDialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/RenameMacroDialog_Title"),
            Inputs =
            [
                new TextOption("name", shell.Properties.Title ?? "")
            ]
        };
        await DialogService.ShowAsync(dialogModel);
        if (dialogModel.Result != DialogResult.Ok)
        {
            return;
        }

        var newTitle = (string)dialogModel.Values![0]!;
        var macroManager = App.GetService<MacroManager>();
        macroManager.UpdateProperties(shell, x => x.Title = newTitle);
    }

    private async void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroShell shell)
        {
            return;
        }

        if (shell.Template?.IsSingleton == true)
        {
            return;
        }

        var dialogModel = new DialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/DeleteMacroDialog_Title"),
            Text = App.Localize($"Poltergeist/Home/DeleteMacroDialog_Text", shell.Title),
            Type = DialogType.YesNo,
        };
        await DialogService.ShowAsync(dialogModel);
        if (dialogModel.Result != DialogResult.Yes)
        {
            return;
        }

        if (App.GetService<INavigationService>().TryCloseTab("macro:" + shell.ShellKey) != true)
        {
            return;
        }

        var macroManager = App.GetService<MacroManager>();
        macroManager.RemoveMacro(shell);
    }

    private async void ChangeIconMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroShell shell)
        {
            return;
        }

        if (shell.Template?.IsSingleton != false)
        {
            return;
        }

        var dialogModel = new InputDialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/ChangeIconDialog_Title"),
            Inputs = [
                new TextOption("icon", shell.Properties.Icon ?? "")
            ]
        };
        await DialogService.ShowAsync(dialogModel);
        if (dialogModel.Result != DialogResult.Ok)
        {
            return;
        }

        var newIcon = (string)dialogModel.Values![0]!;
        var macroManager = App.GetService<MacroManager>();
        macroManager.UpdateProperties(shell, x => x.Icon = newIcon);
    }

    private void StickyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroShell shell)
        {
            return;
        }

        var macroManager = App.GetService<MacroManager>();
        macroManager.UpdateProperties(shell, x => x.IsFavorite = !x.IsFavorite);
    }
}
