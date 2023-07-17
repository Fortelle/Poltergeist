namespace Poltergeist.Common.Utilities.Maths;

public static class CombinatoricsUtil
{

    public static IEnumerable<T[]> GetCombinations<T>(IEnumerable<T> source)
    {
        return Enumerable
            .Range(0, 1 << source.Count())
            .Select(index => source.Where((v, i) => (index & 1 << i) != 0).ToArray());
    }

    public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> source, int length)
    {
        if (length == 1) return source.Select(t => new T[] { t });

        return GetPermutations(source, length - 1)
            .SelectMany(t => source.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    public static IEnumerable<IEnumerable<T>> GetPermutationsWithRepetition<T>(IEnumerable<T> source, int length)
    {
        if (length == 1) return source.Select(t => new T[] { t });

        return GetPermutationsWithRepetition(source, length - 1)
            .SelectMany(t => source, (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
    {
        var random = new Random();
        return source.OrderBy(x => random.NextDouble());
    }
}
