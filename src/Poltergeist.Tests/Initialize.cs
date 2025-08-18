using Poltergeist.Tests;

[assembly: WinUITestTarget(typeof(App))]

namespace Poltergeist.Test;

[TestClass]
public class Initialize
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
    }
}