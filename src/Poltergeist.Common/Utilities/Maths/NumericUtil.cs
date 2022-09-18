namespace Poltergeist.Common.Utilities.Maths;

public static class NumericUtil
{
    public static int NextPrime(int n)
    {
        while (true)
        {
            n++;
            var isPrime = true;
            var m = n / 2;
            for (var i = 2; i <= m; i++)
            {
                if (n % i == 0)
                {
                    isPrime = false;
                    break;
                }
            };
            if (isPrime) return n;
        }
    }

    public static string ToOrdinal(int value)
    {
        var s1 = value % 10;
        var s2 = value % 100;
        return value switch
        {
            < 0 => $"{value}",
            _ when s2 >= 11 && s2 <= 13 => $"{value}th",
            _ when s1 == 1 => $"{value}st",
            _ when s1 == 2 => $"{value}nd",
            _ when s1 == 3 => $"{value}rd",
            _ => $"{value}th"
        };
    }
}
