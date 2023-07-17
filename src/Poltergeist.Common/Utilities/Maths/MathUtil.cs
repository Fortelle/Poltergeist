namespace Poltergeist.Common.Utilities.Maths;

public static class MathUtil
{
    /// <summary>
    /// Returns the square root of the sum of squares of x and y.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
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
