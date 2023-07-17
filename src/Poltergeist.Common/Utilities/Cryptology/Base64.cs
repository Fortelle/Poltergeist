namespace Poltergeist.Common.Utilities.Cryptology;

public static class Base64
{
    public static string ToString(byte[] inArray)
    {
        var s = Convert.ToBase64String(inArray);
        return s;
    }

    public static string ToSafeString(byte[] inArray)
    {
        var s = ToString(inArray);
        s = s.Replace('+', '-').Replace('/', '_');
        return s;
    }

    public static byte[] FromString(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        var data = Convert.FromBase64String(s);
        return data;
    }

    public static byte[] FromString(string s, int multiple)
    {
        var data = FromString(s);
        var length = data.Length / multiple * multiple;
        return data.Take(length).ToArray();
    }

}
