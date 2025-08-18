using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Tests.UITests;

public static class UITestHelper
{
    public static async Task WaitForAppWindowLoaded()
    {
        if (PoltergeistApplication.Current.IsReady)
        {
            return;
        }

        var tcs = new TaskCompletionSource();

        PoltergeistApplication.GetService<AppEventService>().Subscribe<AppLaunchedEvent>(_ =>
        {
            tcs.SetResult();
        });

        await tcs.Task.ConfigureAwait(false);
    }
}
