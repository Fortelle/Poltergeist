using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Modules.HotKeys;

namespace Poltergeist.Tests.UITests.Application;

[TestClass]
public class HotKeyServiceTests
{
    [UITestMethod]
    public async Task Test()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var tcs = new TaskCompletionSource();

        var hotkeyInfo = new HotKeyInformation("test_hotkey")
        {
            HotKey = new(VirtualKey.F12, KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Alt),

            Callback = () =>
            {
                tcs.SetResult();
            },
        };

        var service = PoltergeistApplication.GetService<HotKeyService>();
        service.Add(hotkeyInfo);

        new SendInputHelper()
            .AddScancodeDown(VirtualKey.Control)
            .AddScancodeDown(VirtualKey.Shift)
            .AddScancodeDown(VirtualKey.Menu)
            .AddScancodeDown(VirtualKey.F12)
            .AddScancodeUp(VirtualKey.F12)
            .AddScancodeUp(VirtualKey.Menu)
            .AddScancodeUp(VirtualKey.Shift)
            .AddScancodeUp(VirtualKey.Control)
            .Execute();

        await tcs.Task.ConfigureAwait(false);

        service.Remove(hotkeyInfo);
    }
}
