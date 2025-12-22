namespace System.Linq;

public static class LinqExtensions
{
    public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> match)
    {
        var index = 0;

        foreach (var value in source)
        {
            if (match(value))
            {
                return index;
            }
            index++;
        }

        return -1;
    }
}
