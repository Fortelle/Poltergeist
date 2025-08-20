using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Poltergeist.Modules.Macros;

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
        if (((FrameworkElement)sender).DataContext is not MacroInstanceViewModel instanceViewModel)
        {
            throw new InvalidOperationException();
        }

        if (!instanceViewModel.CanOpen)
        {
            return;
        }

        App.GetService<MacroManager>().OpenPage(instanceViewModel.Instance);
    }

    private void ListViewHeader_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is not string sortKey)
        {
            throw new InvalidOperationException();
        }

        ViewModel.Sort(sortKey);
    }

    private void OpenMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstanceViewModel instanceViewModel)
        {
            throw new InvalidOperationException();
        }

        if (!instanceViewModel.CanOpen)
        {
            return;
        }

        App.GetService<MacroManager>().OpenPage(instanceViewModel.Instance);
    }

    private void EditPropertiesMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstanceViewModel instanceViewModel)
        {
            throw new InvalidOperationException();
        }

        if (!instanceViewModel.CanEdit)
        {
            return;
        }

        _ = ViewModel.EditInstanceProperties(instanceViewModel.Instance);
    }

    private void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstanceViewModel instanceViewModel)
        {
            throw new InvalidOperationException();
        }

        if (!instanceViewModel.CanDelete)
        {
            return;
        }

        _ = ViewModel.DeleteInstance(instanceViewModel.Instance);
    }

    private void CreateShortcutMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstanceViewModel instanceViewModel)
        {
            throw new InvalidOperationException();
        }

        _ = ViewModel.CreateShortcut(instanceViewModel.Instance);
    }

    private void OpenPrivateFolderMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstanceViewModel instanceViewModel)
        {
            throw new InvalidOperationException();
        }

        ViewModel.OpenPrivateFolder(instanceViewModel.Instance);
    }
}
