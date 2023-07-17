using Poltergeist.Common.Utilities;
using Poltergeist.Contracts.Services;
using Poltergeist.Test.Views;

namespace Poltergeist.Test;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : PoltergeistApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnContentLoading(CommandOption[] options)
    {
        base.OnContentLoading(options);

        var navigationService = App.GetService<INavigationService>();
        navigationService.AddInfo(new()
        {
            Key = "debug",
            Header = "Debug",
            IsFooter = true,
            Glyph = "\uEBE8",
            CreateContent = _ => new DebugPage(),
        });
    }
}
