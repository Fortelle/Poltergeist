using Microsoft.UI.Xaml;
using Poltergeist.Modules.Navigation;
using Poltergeist.Tests.UI;

namespace Poltergeist.Tests;

public partial class App : PoltergeistApplication
{
    public App() : base()
    {
        InitializeComponent();

        StartPageKey = "tests";
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.CreateDefaultUI();

        UITestMethodAttribute.DispatcherQueue = DispatcherQueue;

        Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(Environment.CommandLine);
    }

    protected override void OnContentLoading()
    {
        base.OnContentLoading();

        GetService<NavigationService>().AddSidebarItemInfo(new()
        {
            Text = "Tests",
            Icon = new("\uE99A"),
            Position = SidebarItemPosition.Top,
            Navigation = new("tests"),
        });
        GetService<NavigationService>().AddPageInfo(new("tests")
        {
            Header = "Tests",
            Icon = new("\uE99A"),
            CreateContent = (_, _) => new TestPage(),
        });
    }
}
