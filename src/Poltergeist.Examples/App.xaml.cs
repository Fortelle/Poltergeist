using Poltergeist.Automations.Structures;
using Poltergeist.Examples.UI;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.Examples;

public partial class App : PoltergeistApplication
{
    public App()
    {
        InitializeComponent();

        StartPageKey = "example";
    }

    protected override void OnContentLoading()
    {
        base.OnContentLoading();
        var navigationService = GetService<NavigationService>();
        navigationService.AddSidebarItemInfo(new()
        {
            Text = "Example",
            Icon = new GlyphIcon("\ue74c"),
            Position = SidebarItemPosition.Top,
            Navigation = new("example"),
        });
        navigationService.AddPageInfo(new("example")
        {
            Header = "Example",
            Icon = new GlyphIcon("\ue74c"),
            CreateContent = (_, _) => new ExamplePage(),
        });
    }
}
