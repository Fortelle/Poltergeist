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
        if (((FrameworkElement)e.OriginalSource).DataContext is not MacroInstance instance)
        {
            throw new InvalidOperationException();
        }

        if (instance.Template is null)
        {
            return;
        }

        App.GetService<MacroManager>().OpenPage(instance);
    }

    private void ListViewHeader_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var column = Grid.GetColumn((FrameworkElement)sender);
        ViewModel.Sort(column);
    }

    private void OpenMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstance instance)
        {
            throw new InvalidOperationException();
        }

        if (instance.Template is null)
        {
            return;
        }

        App.GetService<MacroManager>().OpenPage(instance);
    }

    private void EditPropertiesMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstance instance)
        {
            return;
        }

        _ = ViewModel.EditInstanceProperties(instance);
    }

    private void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstance instance)
        {
            throw new InvalidOperationException();
        }

        _ = ViewModel.DeleteInstance(instance);
    }

    private void CreateShortcutMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstance instance)
        {
            throw new InvalidOperationException();
        }

        _ = ViewModel.CreateShortcut(instance);
    }

    private void OpenPrivateFolderMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroInstance instance)
        {
            throw new InvalidOperationException();
        }

        ViewModel.OpenPrivateFolder(instance);
    }
}
