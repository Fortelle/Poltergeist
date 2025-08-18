using Poltergeist.Automations.Utilities;

namespace Poltergeist.Tests.UnitTests.Utils;

[TestClass]
public class StringificationUtilTests
{
    [TestMethod]
    public void TestStringify()
    {
        Assert.AreEqual("(null)", StringificationUtil.Stringify(null));
        Assert.AreEqual("foobar", StringificationUtil.Stringify("foobar"));
        Assert.AreEqual("123", StringificationUtil.Stringify(123));
        Assert.AreEqual("{\"foo\":\"bar\",\"baz\":\"qux\"}", StringificationUtil.Stringify(new Dictionary<string, string>() { ["foo"] = "bar", ["baz"] = "qux"}));
        Assert.AreEqual("[\"foo\",\"bar\",\"baz\",\"qux\"]", StringificationUtil.Stringify(new string[] { "foo", "bar", "baz", "qux" }));
    }
}