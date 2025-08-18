using Poltergeist.Modules.CommandLine;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.Application;

[TestClass]
public class CommandLineServiceTests
{
    [UITestMethod]
    public async Task TestPipeMessage()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro();

        var instance = new MacroInstance(macro)
        {
            IsLocked = true,
        };
        PoltergeistApplication.GetService<MacroInstanceManager>().AddInstance(instance);

        CommandLineService.Send(["--macro", instance.InstanceId]);

        await Task.Delay(1000);

        Assert.IsNotNull(PoltergeistApplication.GetService<NavigationService>().TryGetTab(instance.GetPageKey(), out _));

        PoltergeistApplication.GetService<MacroInstanceManager>().RemoveInstance(instance);
    }
}
