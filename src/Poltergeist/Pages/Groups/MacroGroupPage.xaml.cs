using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Poltergeist.Automations.Macros;
using Poltergeist.Contracts.Services;
using Poltergeist.Pages.Groups;
using Poltergeist.Services;

namespace Poltergeist.Views;

public sealed partial class MacroGroupPage : Page
{
    public static readonly NavigationInfo NavigationInfo = new()
    {
        Key = "group",
        Glyph = "\uE838",
        CreateContent = (args, obj) =>
        {
            if (args.Length == 0)
            {
                return null;
            }

            var groupkey = args[0];
            var macroManager = App.GetService<MacroManager>();
            var group = macroManager.Groups.FirstOrDefault(x => x.Key == groupkey);
            if (group is null)
            {
                App.ShowTeachingTip(App.Localize($"Poltergeist/Group/GroupNotExist", groupkey));
                return null;
            }

            var groupPage = new MacroGroupPage(new(group));
            return groupPage;
        },
        CreateHeader = obj => ((MacroGroupPage)obj)!.ViewModel.Group!.Key,
    };

    public MacroGroupViewModel ViewModel { get; }

    public MacroGroupPage(MacroGroupViewModel viewModel)
    {
        ViewModel = viewModel;

        this.InitializeComponent();
    }

    private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is MacroBase macro)
        {
            var nav = App.GetService<INavigationService>();
            nav.NavigateTo("macro:" + macro.Key);
        }
    }
}
