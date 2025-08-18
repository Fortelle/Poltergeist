using Poltergeist.Automations.Structures;

namespace Poltergeist.Tests.UnitTests.Components;

[TestClass]
public class DebouncerTests
{
    [TestMethod]
    public void TestDebounce()
    {
        var buffer = 0;
        var debouncer = new Debouncer(() =>
        {
            buffer++;
        }, TimeSpan.FromMicroseconds(100));

        for (var i = 0; i < 10; i++)
        {
            debouncer.Trigger();
        }

        Assert.AreEqual(0, buffer);

        Thread.Sleep(200);

        Assert.AreEqual(1, buffer);

        for (var i = 0; i < 10; i++)
        {
            debouncer.Trigger();
        }

        Assert.AreEqual(1, buffer);

        Thread.Sleep(200);

        Assert.AreEqual(2, buffer);

    }

    [TestMethod]
    public void TestForceExecute()
    {
        var buffer = 0;
        var debouncer = new Debouncer(() =>
        {
            buffer++;
        }, TimeSpan.FromMicroseconds(100));

        for (var i = 0; i < 10; i++)
        {
            debouncer.Trigger(true);
        }

        Assert.AreEqual(10, buffer);
    }

    [TestMethod]
    public void TestForceExecute2()
    {
        var buffer = 0;
        var debouncer = new Debouncer(() =>
        {
            buffer++;
        }, TimeSpan.FromMicroseconds(100));

        for (var i = 0; i < 10; i++)
        {
            debouncer.Trigger();
        }

        Assert.AreEqual(0, buffer);

        debouncer.Trigger(true);

        Assert.AreEqual(1, buffer);

        Thread.Sleep(200);

        Assert.AreEqual(1, buffer);
    }

}
