namespace Poltergeist.Automations.Utilities.Maths;

public class RandomEx : Random
{
    /// <summary>
    /// Generates a random floating-point number that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
    /// <returns>A floating-point number that is greater than or equal to minValue, and less than maxValue.</returns>
    public double NextDouble(double minValue, double maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} must be greater than or equal to {nameof(minValue)}.");
        }
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
    /// <param name="variables">An integer array of weights.</param>
    /// <returns>An integer of zero-based index.</returns>
    public int NextCategorical(params int[] variables)
    {
        var sum = variables.Sum(x => x < 0 ? 0 : x);
        var sample = Next(sum);
        for (var i = 0; i < variables.Length - 1 - 1; i++)
        {
            if (variables[i] > sample)
            {
                return i;
            }

            if (variables[i] > 0)
            {
                sample -= variables[i];
            }
        }
        return variables.Length - 1;
    }

    /// <summary>
    /// Returns a random index from an array of weights.
    /// </summary>
    /// <param name="variables">An double array of weights.</param>
    /// <returns>An integer of zero-based index.</returns>
    public int NextCategorical(params double[] variables)
    {
        var sum = variables.Sum(x => x < 0 ? 0 : x);
        var sample = Sample() * sum;
        for (var i = 0; i < variables.Length - 1 - 1; i++)
        {
            if (variables[i] > sample)
            {
                return i;
            }

            if (variables[i] > 0)
            {
                sample -= variables[i];
            }
        }
        return variables.Length - 1;
    }

    /// <summary>
    /// Returns a random element of an array according to a weight selector function.
    /// </summary>
    /// <param name="source">The array to return an element from.</param>
    /// <param name="selector">A function to apply to each element.</param>
    /// <returns>An integer of zero-based index.</returns>
    public TSource NextCategorical<TSource>(TSource[] source, Func<TSource, int> selector)
    {
        var variables = source.Select(selector).ToArray();
        var sum = variables.Sum(x => x < 0 ? 0 : x);
        var sample = Sample() * sum;
        for (var i = 0; i < variables.Length - 1 - 1; i++)
        {
            if (variables[i] > sample)
            {
                return source[i];
            }

            if (variables[i] > 0)
            {
                sample -= variables[i];
            }
        }
        return source[^1];
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

        return (int)Math.Ceiling(Math.Log(1.0 - Sample()) / Math.Log(1 - p));

        //var i = 0;
        //while (Sample() >= p)
        //{
        //    i++;
        //}
        //return i + 1;

        /*
         * performance(1m iterations):
         * p    | 0.1 | 0.2 | 0.3 | 0.4 | 0.5 | 0.6 | 0.7 | 0.8 | 0.9
         * loop | 115 |  66 |  48 |  38 |  31 |  27 |  24 |  21 |  17 (ms)
         * log  |  35 |  37 |  39 |  38 |  37 |  37 |  37 |  35 |  33 (ms)
         */
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), by using the central limit theorem.
    /// </summary>
    /// <param name="n">A number of random samples to be used.
    ///     <list type="bullet">
    ///         <item><description>1 for uniform distribution</description></item>
    ///         <item><description>2 for triangular distribution</description></item>
    ///         <item><description>4 for approximate normal distribution</description></item>
    ///     </list>
    /// </param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Central_limit_theorem"/>
    public double NextDoubleCentral(int n)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"'{nameof(n)}' must be greater than zero.");
        }

        double y = 0;
        for (var i = 0; i < n; i++)
        {
            y += Sample();
        }
        y /= n;
        return y;
    }

    /// <summary>
    /// Returns the smallest value in a sequence of random floating-point numbers, in [0.0, 1.0).
    /// </summary>
    /// <param name="n">A number of random samples to be used.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public double NextDoubleMin(int n)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"'{nameof(n)}' must be greater than zero.");
        }

        var y = Sample();
        for (var i = 1; i < n; i++)
        {
            y = Math.Min(y, Sample());
        }
        return y;
    }

    /// <summary>
    /// Returns the largest value in a sequence of random floating-point numbers, in [0.0, 1.0).
    /// </summary>
    /// <param name="n">A number of random samples to be used.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public double NextDoubleMax(int n)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"'{nameof(n)}' must be greater than zero.");
        }

        var y = Sample();
        for (var i = 1; i < n; i++)
        {
            y = Math.Max(y, Sample());
        }
        return y;
    }

    /// <summary>
    /// Generates a random floating-point number from the normal distribution by using the Box-Muller algorithm.
    /// </summary>
    /// <param name="mean">A mean of the distribution.</param>
    /// <param name="stdDev">A standard deviation of the distribution.</param>
    /// <returns>A floating-point number.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Box-Muller_transform"/>
    public double NextBoxMuller(double mean, double stdDev)
    {
        var u1 = SampleWithoutZero();
        var u2 = SampleWithoutZero();
        var r = Math.Sqrt(-2.0 * Math.Log(u1));
        var theta = Math.PI * 2 * u2;

        var z0 = r * Math.Cos(theta) * stdDev + mean;
        return z0;
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the normal distribution by using the Box-Muller algorithm.
    /// </summary>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    public double NextDoubleBoxMuller()
    {
        var mean = 0.5;
        var stdDev = 1.0 / 2.0 / 3.0;
        while (true)
        {
            var u1 = SampleWithoutZero();
            var u2 = SampleWithoutZero();
            var r = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = Math.PI * 2 * u2;

            var z0 = r * Math.Cos(theta) * stdDev + mean;
            if (z0 >= 0 && z0 < 1)
            {
                return z0;
            }

            var z1 = r * Math.Sin(theta) * stdDev + mean;
            if (z1 >= 0 && z1 < 1)
            {
                return z1;
            }
        }
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the skew normal distribution by using the Box-Muller algorithm.
    /// </summary>
    /// <param name="mode">A mode of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Skew_normal_distribution"/>
    public double NextDoubleBoxMuller(double mode)
    {
        return Skew(NextDoubleBoxMuller(), mode);
    }

    /// <summary>
    /// Generates a random floating-point number from the normal distribution by using the Marsaglia polar method.
    /// </summary>
    /// <param name="mean">A mean of the distribution.</param>
    /// <param name="stdDev">A standard deviation of the distribution.</param>
    /// <returns>A floating-point number.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Marsaglia_polar_method"/>
    public double NextPolar(double mean, double stdDev)
    {
        double u1, u2, s;
        do
        {
            u1 = Sample() * 2 - 1;
            u2 = Sample() * 2 - 1;
            s = u1 * u1 + u2 * u2;
        } while (s >= 1 || s == 0);

        var k = Math.Sqrt(-2.0 * Math.Log(s) / s);

        var z0 = u1 * stdDev * k + mean;
        return z0;
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the normal distribution by using the Marsaglia polar method.
    /// </summary>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    public double NextDoublePolar()
    {
        var mean = 0.5;
        var stdDev = 1.0 / 2.0 / 3.0;
        while (true)
        {
            var u1 = Sample() * 2 - 1;
            var u2 = Sample() * 2 - 1;
            var s = u1 * u1 + u2 * u2;
            if (s >= 1 || s == 0)
            {
                continue;
            }

            var k = Math.Sqrt(-2.0 * Math.Log(s) / s);

            var z0 = u1 * stdDev * k + mean;
            if (z0 >= 0 && z0 < 1)
            {
                return z0;
            }

            var z1 = u2 * stdDev * k + mean;
            if (z1 >= 0 && z1 < 1)
            {
                return z1;
            }
        }
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the skew normal distribution by using the Marsaglia polar method.
    /// </summary>
    /// <param name="mode">A mode of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Skew_normal_distribution"/>
    public double NextDoublePolar(double mode)
    {
        return Skew(NextDoublePolar(), mode);
    }

    /// <summary>
    /// Generates a random floating-point number, from the Cauchy distribution with scale = 0.079.
    /// </summary>
    /// <param name="x0">A location parameter of the distribution.</param>
    /// <param name="gamma">A scale parameter of the distribution.</param>
    /// <returns></returns>
    /// <see href="https://en.wikipedia.org/wiki/Cauchy_distribution"/>
    public double NextCauchy(double x0, double gamma)
    {
        var u = Sample();
        var r = Math.Tan(Math.PI * (u - 0.5));
        var z = x0 + r * gamma;
        return z;
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the cauchy distribution with scale = 0.079.
    /// </summary>
    /// <param name="mode">A mode of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    public double NextDoubleCauchy(double mode = 0.5)
    {
        var scale = 0.079; // 90%
        double x;
        do
        {
            x = NextCauchy(.5, scale);
        } while (x < 0 || x >= 1);
        return Skew(x, mode);
    }

    /// <summary>
    /// Generates a random floating-point number from the Laplace distribution.
    /// </summary>
    /// <param name="mu">A location parameter of the distribution.</param>
    /// <param name="b">A scale parameter of the distribution.</param>
    /// <returns>A floating-point number.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Laplace_distribution"/>
    public double NextLaplace(double mu, double b)
    {
        var u = SampleWithoutZero() - 0.5;
        return mu - b * Math.Sign(u) * Math.Log(1 - 2 * Math.Abs(u));
    }

    /// <summary>
    /// Generates a random floating-point number, in (0.0, 1.0), from the Laplace distribution with mu = 0.5 and b = 0.073.
    /// </summary>
    /// <param name="mode">A mode of the distribution.</param>
    /// <returns>A floating-point number that is greater than 0.0, and less than 1.0.</returns>
    public double NextDoubleLaplace(double mode = 0.5)
    {
        var mu = 0.5;
        var b = 0.073; // for 99.9%
        double x;
        do
        {
            x = NextLaplace(mu, b);
        } while (x < 0 || x >= 1);

        return Skew(x, mode);
    }

    /// <summary>
    /// Generates a random floating-point number from the Rayleigh distribution.
    /// </summary>
    /// <param name="sigma">A scale parameter of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than Double.MaxValue.</returns>
    /// <see cref="https://en.wikipedia.org/wiki/Rayleigh_distribution"/>
    public double NextRayleigh(double sigma)
    {
        var u = SampleWithoutZero();
        var r = Math.Sqrt(-2.0 * Math.Log(u));
        var z = r * sigma;
        return z;
    }

    /// <summary>
    /// Generates a random floating-point number, in (0.0, 1.0), from the Rayleigh distribution with stdDev = 0.25.
    /// </summary>
    /// <param name="stdDev">A standard deviation of the distribution.</param>
    /// <returns>A floating-point number that is greater than 0.0, and less than 1.0.</returns>
    public double NextDoubleRayleigh(double stdDev = 0.25) // 99.9%
    {
        while (true)
        {
            var x = NextRayleigh(stdDev);
            if (x < 1)
            {
                return x;
            }
        }
    }

    /// <summary>
    /// Generates a random floating-point number from the Exponential distribution.
    /// </summary>
    /// <param name="lambda">A rate parameter of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than Double.MaxValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <see cref="https://en.wikipedia.org/wiki/Exponential_distribution"/>
    public double NextExponential(double lambda)
    {
        if (lambda <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lambda), $"'{nameof(lambda)}' must be greater than 0.");
        }

        return -Math.Log(1 - Sample()) / lambda;
    }

    /// <summary>
    /// Generates a random floating-point number, in (0.0, 1.0), from the Exponential distribution with lambda = 7.
    /// </summary>
    /// <param name="lambda">A rate parameter of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than Double.MaxValue.</returns>
    public double NextDoubleExponential(double lambda = 7) // 5 for 99.3%, 6 for 99.7%, 7 for 99.9%
    {
        while (true)
        {
            var x = NextExponential(lambda);
            if (x < 1)
            {
                return x;
            }
        }
    }

    /// <summary>
    /// Generates a random floating-point number, in [0.0, 1.0), from the triangular distribution.
    /// </summary>
    /// <param name="mode">A mode of the distribution.</param>
    /// <returns>A floating-point number that is greater than or equal to 0.0, and less than or equal to 1.0.</returns>
    /// <see href="https://en.wikipedia.org/wiki/Triangular_distribution"/>
    public double NextDoubleTriangular(double mode = 0.5)
    {
        if (mode < 0 || mode > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), $"'{nameof(mode)}' must be greater than or equal to 0 and less than or equal to 1.");
        }

        var x = Sample();
        return x < mode
            ? Math.Sqrt(x * mode)
            : 1 - Math.Sqrt((1 - x) * (1 - mode));
    }


    /// <summary>
    /// Returns random elements from an enumerable collection with unknown size.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The sequence to return elements from.</param>
    /// <param name="k">A number of elements.</param>
    /// <returns>An array that contains the specified number of elements that are randomly picked from the source.</returns>
    /// <see cref="https://en.wikipedia.org/wiki/Reservoir_sampling"/>
    public T[] ReservoirSample<T>(IEnumerable<T> source, int k)
    {
        var ret = new T[k];
        var i = 0;
        foreach (var x in source)
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

    /// <summary>
    /// Returns a random element from an enumerable collection with unknown size.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The sequence to return elements from.</param>
    /// <returns>An element that is randomly picked from the source.</returns>
    /// <see cref="https://en.wikipedia.org/wiki/Reservoir_sampling"/>
    public T? ReservoirSample<T>(IEnumerable<T> source)
    {
        return ReservoirSample(source, 1).FirstOrDefault();
    }

    /// <summary>
    /// Returns a random floating-point number in (0.0, 1.0).
    /// </summary>
    /// <returns>A floating-point number that is greater than 0.0 and less than 1.0.</returns>
    private double SampleWithoutZero()
    {
        double sample;
        do
        {
            sample = Sample();
        }
        while (sample <= double.Epsilon);

        return sample;
    }

    private double Skew(double x, double mode)
    {
        if (mode < 0 || mode >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), $"'{nameof(mode)}' must be greater than or equal to 0 and less than 1.");
        }
        else if (mode == 0.5)
        {
            return x;
        }

        // make the distribution continuous 
        if (
            mode < 0.5 && x < 0.5 && x != 0 && NextBernoulli((0.5 - mode) / 0.5) ||
            mode >= 0.5 && x >= 0.5 && NextBernoulli((mode - 0.5) / 0.5)
            )
        {
            x = 1 - x;
        }

        if (x < 0.5)
        {
            return x / 0.5 * mode;
        }
        else
        {
            return mode + (x - 0.5) / 0.5 * (1 - mode);
        }
    }

}
