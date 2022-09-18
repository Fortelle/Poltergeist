using System;
using System.Collections.Generic;
using System.Linq;

namespace Poltergeist.Common.Utilities.Maths;

public class RandomEx : Random
{
    public double NextDouble(double minValue, double maxValue)
    {
        return minValue + Sample() * (maxValue - minValue);
    }

    /// <summary>
    /// Generates a random boolean from the uniform distribution.
    /// </summary>
    /// <returns>A boolean.</returns>
    public bool NextBoolean()
    {
        return NextBernoulli(0.5);
    }

    /// <summary>
    /// Generates a random boolean with a probability.
    /// </summary>
    /// <param name="prob">A probability of true.</param>
    /// <returns>A boolean.</returns>
    public bool NextBernoulli(double prob)
    {
        return prob switch
        {
            >= 1 => true,
            <= 0 => false,
            _ => Sample() < prob,
        };
    }

    /// <summary>
    /// Returns a random index from an array of weights.
    /// </summary>
    /// <param name="variables">An integer array of variables.</param>
    /// <returns>An integer of zero-based index.</returns>
    public int NextCategorical(params int[] variables)
    {
        variables = variables.Select(x => x < 0 ? 0 : x).ToArray();
        var sum = variables.Sum();
        var sample = Next(sum);
        for (var i = 0; i < variables.Length - 1 - 1; i++)
        {
            if (variables[i] > sample) return i;
            sample -= variables[i];
        }
        return variables.Length - 1;
    }

    /// <summary>
    /// Returns a random index from an array of weights.
    /// </summary>
    /// <param name="variables">An double array of variables.</param>
    /// <returns>An integer of index.</returns>
    public int NextCategorical(params double[] variables)
    {
        variables = variables.Select(x => x < 0 ? 0 : x).ToArray();
        var sum = variables.Sum();
        var sample = Sample() * sum;
        for (var i = 0; i < variables.Length - 1 - 1; i++)
        {
            if (variables[i] > sample) return i;
            sample -= variables[i];
        }
        return variables.Length - 1;
    }

    public TSource NextCategorical<TSource>(TSource[] source, Func<TSource, int> selector)
    {
        var variables = source.Select(selector).ToArray();
        var sum = variables.Sum();
        var sample = Sample() * sum;
        var index = variables.Length - 1 - 1;
        for (var i = 0; i < index; i++)
        {
            if (variables[i] > sample)
            {
                index = i;
                break;
            }
            sample -= variables[i];
        }
        return source[index];
    }

    /// <summary>
    /// Generates a random integer from the geometric distribution.
    /// </summary>
    /// <param name="p">A probability of success in a Bernoulli trial.</param>
    /// <returns>An integer of total trials it takes to get one success.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Geometric_distribution"/>
    public int NextGeometric(double p)
    {
        if (p < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(p), $"The probability must be greater than zero.");
        }
        else if (p >= 1)
        {
            return 1;
        }

        var i = 0;
        while (Sample() >= p)
        {
            i++;
        }
        return i + 1;
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), by using the central limit theorem.
    /// </summary>
    /// <param name="n">A number of random samples to be used.
    ///     <list type="bullet">
    ///         <item><description>1 for uniform distribution</description></item>
    ///         <item><description>2 for triangular distribution</description></item>
    ///         <item><description>4 for normal distribution</description></item>
    ///     </list>
    /// </param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Central_limit_theorem"/>
    public double NextCentral(int n = 1)
    {
        double y = 0;
        for (var i = 0; i < n; i++)
        {
            y += Sample();
        }
        y /= n;
        return y;
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the normal distribution by using the polar algorithm.
    /// </summary>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Box-Muller_transform"/>
    public double NextBoxMuller()
    {
        var mean = 0.5;
        var stdDev = Math.Max(mean, 1 - mean) / 3.5;
        while (true)
        {
            var u1 = Sample();
            var u2 = Sample();
            var r = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = Math.PI * 2 * u2;

            var z0 = r * Math.Cos(theta) * stdDev + mean;
            if (z0 >= 0 && z0 < 1) return z0;
            var z1 = r * Math.Sin(theta) * stdDev + mean;
            if (z1 >= 0 && z1 < 1) return z1;
        }
    }

    public double NextBoxMuller(double mean, double stdDev)
    {
        var u1 = Sample();
        var u2 = Sample();
        var r = Math.Sqrt(-2.0 * Math.Log(u1));
        var theta = Math.PI * 2 * u2;

        var z0 = r * Math.Cos(theta) * stdDev + mean;
        return z0;
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the normal distribution by using the Marsaglia polar method.
    /// </summary>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Marsaglia_polar_method"/>
    public double NextPolar()
    {
        var mean = 0.5;
        var stdDev = Math.Max(mean, 1 - mean) / 3.5;
        while (true)
        {
            var u = Sample() * 2 - 1;
            var v = Sample() * 2 - 1;
            var s = u * u + v * v;
            if (s >= 1 || s == 0) continue;
            var r = Math.Sqrt(-2.0 * Math.Log(s) / s);

            var z0 = u * stdDev * r + mean;
            if (z0 >= 0 && z0 < 1) return z0;
            var z1 = v * stdDev * r + mean;
            if (z1 >= 0 && z1 < 1) return z1;
        }
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the triangular distribution.
    /// </summary>
    /// <param name="mode">A mode of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Triangular_distribution"/>
    public double NextTriangular(double mode = 0.5)
    {
        var x = Sample();
        return x < mode
            ? Math.Sqrt(x * mode)
            : 1 - Math.Sqrt((1 - x) * (1 - mode));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    /// <see cref="https://en.wikipedia.org/wiki/Reservoir_sampling"/>
    public T[] ReservoirSample<T>(IEnumerable<T> data, int k)
    {
        var ret = new T[k];
        var i = 0;
        foreach (var x in data)
        {
            if (i < k)
            {
                ret[i] = x;
            }
            else
            {
                var sample = Next(i);
                if (sample < k)
                {
                    ret[sample] = x;
                }
            }
            i++;
        }
        return ret;
    }


    public double NextSector(bool gr = false)
    {
        var x = Sample();
        var y = Math.Sqrt(1 - Math.Pow(x, 2));
        if (gr) y = 1 - y;
        return y;
    }

    public double NextBeta(double a, double b)
    {
        var u = GetGamma(a, 1.0);
        var v = GetGamma(b, 1.0);
        return u / (u + v);
    }

    private double GetGamma(double k, double theta)
    {
        if (k <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(k), "k must be positive.");
        }
        else if (k >= 1.0)
        {
            var d = k - 1.0 / 3.0;
            var c = 1.0 / Math.Sqrt(9.0 * d);
            double x, v;
            for (; ; )
            {
                do
                {
                    x = Sample();
                    v = 1.0 + c * x;
                }
                while (v <= 0.0);
                v = v * v * v;
                var u = Sample();
                var xsquared = x * x;

                if (u < 1.0 - .0331 * xsquared * xsquared || Math.Log(u) < 0.5 * xsquared + d * (1.0 - v + Math.Log(v)))
                    return theta * d * v;
            }
        }
        else
        {
            var g = GetGamma(k + 1.0, 1.0);
            var w = Sample();
            return theta * g * Math.Pow(w, 1.0 / k);
        }
    }

    public double NextTest(double lambda = 1)
    {
        var u = Sample();
        var t = -Math.Log(u) / lambda;
        return t;
    }

}
