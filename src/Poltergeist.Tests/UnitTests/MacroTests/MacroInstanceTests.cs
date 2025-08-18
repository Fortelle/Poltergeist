using System.Security.Cryptography;
using System.Text;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroInstanceTests
{
    [TestMethod]
    public void TestInstanceId()
    {
        var macro = new TestMacro();

        var instance1 = new MacroInstance(macro);
        var instance2 = new MacroInstance(macro);

        Assert.AreNotEqual(instance1.InstanceId, instance2.InstanceId);
    }

    [TestMethod]
    public void TestStaticInstances()
    {
        var macro = new TestMacro();

        var instance = MacroInstance.CreateStaticInstance(macro);

        var bytes = Encoding.UTF8.GetBytes(macro.Key);
        var hash = MD5.HashData(bytes);
        var instanceId = new Guid(hash).ToString();

        Assert.AreEqual(instanceId, instance.InstanceId);
    }
}
