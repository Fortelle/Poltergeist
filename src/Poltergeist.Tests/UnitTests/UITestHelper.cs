namespace Poltergeist.Tests.UITests;

public static class UnitTestHelper
{
    public static string GetTempFileName()
    {
        var filename = Path.GetRandomFileName();
        var filepath = Path.Combine(PoltergeistApplication.Paths.DocumentDataFolder, "Tests", "Temp", filename);
        return filepath;
    }
}
