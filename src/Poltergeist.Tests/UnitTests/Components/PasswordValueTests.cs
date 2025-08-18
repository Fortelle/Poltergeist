using System.Text;
using System.Text.Json;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Tests.UnitTests.Components;

[TestClass]
public class PasswordValueTests
{
    [TestMethod]
    public void TestValue()
    {
        var password = "123456";
        var passwordValue = new PasswordValue(password);

        Assert.AreEqual(password, passwordValue.Plaintext);
        Assert.AreEqual(password, passwordValue.ToString());
    }

    [TestMethod]
    public void TestSerialize()
    {
        var password = "123456";
        var bytes = Encoding.UTF8.GetBytes(password);
        var base64 = Convert.ToBase64String(bytes);

        var passwordValue = new PasswordValue(password);
        var jsonText = JsonSerializer.Serialize(passwordValue).Trim('"');

        Assert.AreEqual(jsonText, base64);
    }

    [TestMethod]
    public void TestDeserialize()
    {
        var password = "123456";
        var bytes = Encoding.UTF8.GetBytes(password);
        var base64 = Convert.ToBase64String(bytes);

        var passwordValue = JsonSerializer.Deserialize<PasswordValue>('"' + base64 + '"');

        Assert.AreEqual(password, passwordValue!.Plaintext);
    }
}
