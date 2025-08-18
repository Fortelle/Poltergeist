using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Tests.UnitTests.Utils;

[TestClass]
public class LogicalUtilTests
{
    [TestMethod]
    public void TestIsTruthy()
    {
        Assert.IsFalse(LogicalUtil.IsTruthy(null));
        Assert.IsFalse(LogicalUtil.IsTruthy(new Exception()));
        Assert.IsFalse(LogicalUtil.IsTruthy(false));
        Assert.IsFalse(LogicalUtil.IsTruthy(""));
        Assert.IsFalse(LogicalUtil.IsTruthy(Array.Empty<int>()));
        Assert.IsFalse(LogicalUtil.IsTruthy(new List<int>()));
        Assert.IsFalse(LogicalUtil.IsTruthy(0));
        Assert.IsFalse(LogicalUtil.IsTruthy(TimeSpan.Zero));

        Assert.IsTrue(LogicalUtil.IsTruthy(" "));
        Assert.IsTrue(LogicalUtil.IsTruthy(1));
    }

}
