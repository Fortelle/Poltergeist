using Poltergeist.Modules.Navigation;
using Poltergeist.Test.Views;

namespace Poltergeist.Test;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : PoltergeistApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override void ConfigureNavigations(INavigationService navigationService)
    {
        base.ConfigureNavigations(navigationService);

        navigationService.AddInfo(new()
        {
            Key = "test",
            Header = "Test",
            Icon = new("\uE99A"),
            PositionInSidebar = NavigationItemPosition.Bottom,
            CreateContent = (_, _) => new TestPage(),
        });
    }

}
