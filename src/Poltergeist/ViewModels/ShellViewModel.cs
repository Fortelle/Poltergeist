using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Macros;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    public IList MenuItems;
    public MacroGroup[] Groups { get; set; }
    public bool IsDebug => Debugger.IsAttached;

    private bool _isReady;
    public bool IsReady { get => _isReady; set => SetProperty(ref _isReady, value); }

    private bool _isConsoleLoaded;
    public bool IsConsoleLoaded { get => _isConsoleLoaded; set => SetProperty(ref _isConsoleLoaded, value); }

    private Page _currentPage;
    public Page CurrentPage
    {
        get => _currentPage;
        set
        {
            SetProperty(ref _currentPage, value);
            var pageName = value.Name;
            if (MenuItems == null) return;
            if (SelectedItem?.Tag.ToString() == pageName) return;
            SelectedItem = MenuItems.OfType<ModernWpf.Controls.NavigationViewItem>().FirstOrDefault(x => x.Tag.ToString() == pageName);
            if (SelectedItem != null)
            {
                SelectedItem.IsSelected = true;
            }
            if(pageName == "console")
            {
                IsConsoleLoaded = true;
            }
        }
    }

    private ModernWpf.Controls.NavigationViewItem _selectedItem;
    public ModernWpf.Controls.NavigationViewItem SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ShellViewModel(NavigationService navigationService, MacroManager macroManager)
    {
        navigationService.Navigated += OnNavigated;
        Groups = macroManager.Groups.ToArray();
        IsReady = true;
    }

    private void OnNavigated(Page page)
    {
        CurrentPage = page;
    }

}
