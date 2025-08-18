using Poltergeist.Modules.Events;

namespace Poltergeist.Tests.UITests.Application;

[TestClass]
public class AppEventServiceTests
{
    private class TestEvent : AppEvent
    {
    }

    [StrictOneTime]
    private class StrictOneTimeTestEvent : AppEvent
    {
    }


    [UITestMethod]
    public async Task TestSubscription()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var eventService = PoltergeistApplication.GetService<AppEventService>();

        var value = 0;
        void handler(TestEvent _)
        {
            value++;
        }

        eventService.Subscribe<TestEvent>(handler);
        eventService.Publish<TestEvent>();
        Assert.AreEqual(1, value);
        eventService.Publish<TestEvent>();
        Assert.AreEqual(2, value);
        eventService.Unsubscribe<TestEvent>(handler);
        eventService.Publish<TestEvent>();
        Assert.AreEqual(2, value);
    }

    [UITestMethod]
    public async Task TestOnceSubscription()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var eventService = PoltergeistApplication.GetService<AppEventService>();

        var value = 0;
        eventService.Subscribe<TestEvent>(_ => value++);
        eventService.Subscribe<TestEvent>(_ => value++, new() { Once = true });

        eventService.Publish<TestEvent>();
        Assert.AreEqual(2, value);

        eventService.Publish<TestEvent>();
        Assert.AreEqual(3, value);
    }

    [UITestMethod]
    public async Task TestPriority()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var eventService = PoltergeistApplication.GetService<AppEventService>();

        {
            var value = "";
            eventService.Subscribe<TestEvent>(_ => value = "second", new() { Priority = 1, Once = true });
            eventService.Subscribe<TestEvent>(_ => value = "first", new() { Priority = 2, Once = true });
            eventService.Publish<TestEvent>();
            Assert.AreEqual("second", value);
        }

        {
            var value = "";
            eventService.Subscribe<TestEvent>(_ => value = "first", new() { Priority = 2, Once = true });
            eventService.Subscribe<TestEvent>(_ => value = "second", new() { Priority = 1, Once = true });
            eventService.Publish<TestEvent>();
            Assert.AreEqual("second", value);
        }
    }

    [UITestMethod]
    public async Task TestOneTimeEvent()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var eventService = PoltergeistApplication.GetService<AppEventService>();

        eventService.Publish<StrictOneTimeTestEvent>();
        Assert.ThrowsExactly<InvalidOperationException>(() => eventService.Publish<StrictOneTimeTestEvent>());
    }
}
