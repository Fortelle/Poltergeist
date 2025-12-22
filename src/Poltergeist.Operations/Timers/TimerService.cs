using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Timers;

public class TimerService : MacroService
{
    private readonly DelayOptions DefaultOptions;

    private readonly DistributionService DistributionService;

    public TimerService(
        MacroProcessor processor,
        DistributionService distributionService,
        IOptions<DelayOptions> options
        ) : base(processor)
    {
        DistributionService = distributionService;
        DefaultOptions = options.Value;
    }

    public void Delay(DelayToken token, DelayOptions? options = null)
    {
        Logger.Trace($"Executing delay.", new { token, options });
        Logger.IncreaseIndent();

        var timeout = GetTimeout(token, options);
        if (timeout > 0)
        {
            Thread.Sleep(timeout);
            Logger.Debug($"Delayed for {timeout}ms.");
        }

        Processor.ThrowIfInterrupted();

        Logger.DecreaseIndent();
    }

    public async Task DelayAsync(DelayToken token, DelayOptions? options = null)
    {
        Logger.Trace($"Executing delay.", new { token, options });
        Logger.IncreaseIndent();

        var timeout = GetTimeout(token, options);
        if (timeout > 0)
        {
            await Task.Delay(timeout);
            Logger.Debug($"Delayed for {timeout}ms.");
        }

        Processor.ThrowIfInterrupted();

        Logger.DecreaseIndent();
    }

    public int GetTimeout(DelayToken token, DelayOptions? options = null)
    {
        return token switch
        {
            PreciseDelay preciseDelay => GetTimeoutInternal(preciseDelay),
            CoarseDelay coarseDelay => GetTimeoutInternal(coarseDelay, options),
            RangeDelay rangeDelay => GetTimeoutInternal(rangeDelay, options),
            _ => throw new NotSupportedException(),
        };
    }

    private int GetTimeoutInternal(PreciseDelay preciseDelay)
    {
        var milliseconds = preciseDelay.Milliseconds;
        Logger.Trace($"Calculated delay timeout: {milliseconds}ms.", new { preciseDelay });
        return milliseconds;
    }

    private int GetTimeoutInternal(CoarseDelay coarseDelay, DelayOptions? options)
    {
        var milliseconds = coarseDelay.Milliseconds;
        if (milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(coarseDelay), "Timeout cannot be negative.");
        }

        if (milliseconds == 0)
        {
            Logger.Trace($"Calculated delay timeout: {milliseconds}ms.", new { coarseDelay });
            return milliseconds;
        }

        var floating = coarseDelay.Floating ?? options?.Floating ?? DefaultOptions?.Floating ?? false;
        if (!floating)
        {
            Logger.Trace($"Calculated delay timeout: {milliseconds}ms.", new { coarseDelay, floating });
            return milliseconds;
        }

        var (rateMin, rateMax) = coarseDelay.FloatingRange ?? options?.FloatingRange ?? DefaultOptions?.FloatingRange ?? (1, 1);
        var min = milliseconds * rateMin;
        var max = milliseconds * rateMax;
        var distribution = coarseDelay.FloatDistribution ?? options?.FloatDistribution ?? DefaultOptions?.FloatDistribution ?? RangeDistributionType.Uniform;
        var sample = DistributionService.NextDouble(distribution);
        var timeout = (int)((max - min) * sample + min);

        Logger.Trace($"Calculated delay timeout: {timeout}ms.", new { milliseconds, rateMin, rateMax, distribution, sample });
        return timeout;
    }

    private int GetTimeoutInternal(RangeDelay rangeDelay, DelayOptions? options)
    {
        var min = rangeDelay.StartMilliseconds;
        var max = rangeDelay.EndMilliseconds;

        if (min < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rangeDelay), "Timeout cannot be negative.");
        }
        if (max < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rangeDelay), "Timeout cannot be negative.");
        }
        if (max < min)
        {
            throw new ArgumentOutOfRangeException(nameof(rangeDelay), "The end of the range cannot be less than the start of the range.");
        }

        if (min == 0 && max == 0)
        {
            Logger.Trace($"Calculated delay timeout: {0}ms.", new { rangeDelay });
            return 0;
        }

        if (min == max)
        {
            Logger.Trace($"Calculated delay timeout: {min}ms.", new { rangeDelay });
            return min;
        }

        var floating = rangeDelay.Floating ?? options?.Floating ?? DefaultOptions?.Floating ?? false;
        if (!floating)
        {
            var avg = (max + min) / 2;
            Logger.Trace($"Calculated delay timeout: {avg}ms.", new { rangeDelay, floating });
            return avg;
        }

        var distribution = rangeDelay.RangeDistribution ?? options?.RangeDistribution ?? DefaultOptions?.RangeDistribution ?? RangeDistributionType.Uniform;
        var sample = DistributionService.NextDouble(distribution);
        var timeout = (int)((max - min) * sample + min);

        Logger.Trace($"Calculated delay timeout: {timeout}ms.", new { rangeDelay, distribution, sample });
        return timeout;
    }
}
