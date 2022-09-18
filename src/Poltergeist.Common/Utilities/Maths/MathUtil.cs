using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poltergeist.Common.Utilities.Maths;

public static class MathUtil
{
    public static int Clamp(int source, int min, int max)
    {
        var isReversed = min.CompareTo(max) > 0;
        var smallest = isReversed ? max : min;
        var biggest = isReversed ? min : max;

        return source.CompareTo(smallest) < 0 ? smallest : source.CompareTo(biggest) > 0 ? biggest : source;
    }

    public static double Hypot(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }

    public static IEnumerable<int> Accumulate<TIn>(this IEnumerable<TIn> source, Func<TIn, int> func)
    {
        var accumulator = 0;
        foreach (var item in source)
        {
            var value = func(item);
            accumulator += value;
            yield return accumulator;
        }
    }
}
