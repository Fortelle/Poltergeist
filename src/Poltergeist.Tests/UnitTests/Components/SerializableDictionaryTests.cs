using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Tests.UnitTests.Components;

[TestClass]
public class SerializableParameterValueCollectionTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        IncludeFields = true,
    };

    [TestMethod]
    public void TestDeserialize()
    {
        var dict = new SerializableParameterValueCollection()
        {
            ["bool"] = true,
            ["int"] = 1,
            ["int[]"] = new int[] { 1, 2, 3 },
            ["string"] = "foobar",
            ["string[]"] = new string[] { "foo", "bar" },
            ["enum"] = DayOfWeek.Monday,
            ["enum[]"] = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
            ["class"] = new Version(1, 2, 3),
            ["valuetuple"] = (1, true),
            ["tuple"] = Tuple.Create(1, true),
        };

        var text = JsonSerializer.Serialize(dict, SerializerOptions);
        var newDict = JsonSerializer.Deserialize<SerializableParameterValueCollection>(text, SerializerOptions)!;

        Check(newDict);
    }

    private void Check(SerializableParameterValueCollection dict)
    {
        Assert.IsTrue(dict.Get<bool>("bool") is true);
        Assert.IsTrue(dict.Get<int>("int") is 1);
        Assert.IsTrue(dict.Get<int[]>("int[]") is [1, 2, 3]);
        Assert.IsTrue(dict.Get<string>("string") is "foobar");
        Assert.IsTrue(dict.Get<string[]>("string[]") is ["foo", "bar"]);
        Assert.IsTrue(dict.Get<DayOfWeek>("enum") is DayOfWeek.Monday);
        Assert.IsTrue(dict.Get<DayOfWeek[]>("enum[]") is [DayOfWeek.Monday, DayOfWeek.Tuesday]);
        Assert.IsTrue(dict.Get<Version>("class") is { Major: 1, Minor: 2, Build: 3 });
        Assert.IsTrue(dict.Get<(int, bool)>("valuetuple") is (1, true));
        Assert.IsTrue(dict.Get<Tuple<int, bool>>("tuple") is (1, true));
    }
}
