using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class SessionStorageTests
{
    private class DisposableObject : object, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [TestMethod]
    public void TestDispose()
    {
        var obj = new DisposableObject();

        var macro = new BasicMacro()
        {
            Execute = (args) =>
            {
                args.Processor.SessionStorage.Add("test_object", obj);
            },
        };

        MacroProcessor.Execute(macro);

        Assert.IsTrue(obj.IsDisposed);
    }

}
