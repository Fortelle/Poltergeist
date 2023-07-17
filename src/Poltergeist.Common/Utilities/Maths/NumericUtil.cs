namespace Poltergeist.Common.Utilities.Maths;

public static class NumericUtil
{
    public static bool IsPrime(int n)
    {
        if (n <= 1)
        {
            return false;
        }

        var m = n / 2;
        for (var i = 2; i <= m; i++)
        {
            if (n % i == 0)
            {
                return false;
            }
        };
        return true;
    }

    public static int NextPrime(int n)
    {
        if (n <= 1)
        {
            n = 1;
        }

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
            if (isPrime)
            {
                return n;
            }
        }
    }

    public static string ToOrdinal(int value)
    {
        if(value <= 0)
        {
            return value.ToString();
        }

        if((value % 100) is 11 or 12 or 13)
        {
            return $"{value}th";
        }

        return (value % 10) switch
        {
            1 => $"{value}st",
            2 => $"{value}nd",
            3 => $"{value}rd",
            _ => $"{value}th"
        };
    }
}
