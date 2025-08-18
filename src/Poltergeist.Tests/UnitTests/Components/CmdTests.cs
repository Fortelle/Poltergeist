using Poltergeist.Automations.Utilities;

namespace Poltergeist.Tests.UnitTests.Components;

[TestClass]
public class CmdTests
{
    [TestMethod]
    public void TestCmdHost()
    {
        using var host = new CmdHost(@"C:\");
        host.Start();

        host.TryExecute("cd", out var text);
        Assert.AreEqual(@"C:\", text, true);
    }
}
