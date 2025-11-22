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

    public void Delay(int milliseconds, DelayOptions? options = null)
    {
        Logger.Trace($"Executing delay: {milliseconds}ms.", new { options });
        Logger.IncreaseIndent();

        var timeout = GetTimeout(milliseconds, options);
        DoDelay(timeout);

        Logger.DecreaseIndent();
    }

    public void Delay(int min, int max, DelayOptions? options = null)
    {
        Logger.Trace($"Executing delay: {min}-{max}ms.", new { options });
        Logger.IncreaseIndent();

        var timeout = GetTimeout(min, max, options);
        DoDelay(timeout);

        Logger.DecreaseIndent();
    }

    public void Delay(TimeSpan timespan, DelayOptions? options = null) => Delay((int)timespan.TotalMilliseconds, options);

    public void Delay(TimeSpanRange range, DelayOptions? options = null) => Delay((int)range.Start.TotalMilliseconds, (int)range.End.TotalMilliseconds, options);

    public async Task DelayAsync(int milliseconds, DelayOptions? options = null)
    {
        Logger.Trace($"Executing delay: {milliseconds}ms.", new { options });
        Logger.IncreaseIndent();

        var timeout = GetTimeout(milliseconds, options);
        await DoDelayAsync(timeout);

        Logger.DecreaseIndent();
    }

    public async Task DelayAsync(int min, int max, DelayOptions? options = null)
    {
        Logger.Trace($"Executing delay: {min}-{max}ms.", new { options });
        Logger.IncreaseIndent();

        var timeout = GetTimeout(min, max, options);
        await DoDelayAsync(timeout);

        Logger.DecreaseIndent();
    }

    public async Task DelayAsync(TimeSpan timespan, DelayOptions? options = null) => await DelayAsync((int)timespan.TotalMilliseconds, options);

    public async Task DelayAsync(TimeSpanRange range, DelayOptions? options = null) => await DelayAsync((int)range.Start.TotalMilliseconds, (int)range.End.TotalMilliseconds, options);

    private void DoDelay(int timeout)
    {
        if (timeout > 0)
        {
            Thread.Sleep(timeout);
        }

        Logger.Debug($"Delayed for {timeout}ms.");

        Processor.ThrowIfInterrupted();
    }

    private async Task DoDelayAsync(int timeout)
    {
        if (timeout > 0)
        {
            await Task.Delay(timeout);
        }

        Logger.Debug($"Delayed for {timeout}ms.");

        Processor.ThrowIfInterrupted();
    }

    public int GetTimeout(int milliseconds, DelayOptions? options)
    {
        if (milliseconds == 0)
        {
            Logger.Trace($"Calculated delay timeout: {milliseconds}ms.", new { milliseconds });
            return milliseconds;
        }

        var floating = options?.Floating ?? DefaultOptions?.Floating ?? false;
        if (!floating)
        {
            Logger.Trace($"Calculated delay timeout: {milliseconds}ms.", new { floating });
            return milliseconds;
        }

        var (rateMin, rateMax) = options?.FloatingRange ?? DefaultOptions?.FloatingRange ?? (1, 1);
        var distribution = options?.FloatDistribution ?? DefaultOptions?.FloatDistribution ?? RangeDistributionType.Uniform;

        var min = milliseconds * rateMin;
        var max = milliseconds * rateMax;

        var sample = DistributionService.NextDouble(distribution);
        var value = (int)((max - min) * sample + min);

        Logger.Trace($"Calculated delay timeout: {value}ms.", new { milliseconds, rateMin, rateMax, distribution, sample });
        return value;
    }

    public int GetTimeout(int min, int max, DelayOptions? options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(min, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, min);

        if (min == 0 && max == 0)
        {
            Logger.Trace($"Calculated delay timeout: {0}ms.", new { min, max });
            return 0;
        }

        if (min == max)
        {
            Logger.Trace($"Calculated delay timeout: {min}ms.", new { min, max });
            return min;
        }

        var floating = options?.Floating ?? DefaultOptions?.Floating ?? false;
        if (!floating)
        {
            var avg = (max - min) / 2;
            Logger.Trace($"Calculated delay timeout: {avg}ms.", new { floating });
            return avg;
        }

        var distribution = options?.RangeDistribution ?? DefaultOptions?.RangeDistribution ?? RangeDistributionType.Uniform;

        var sample = DistributionService.NextDouble(distribution);
        var value = (int)((max - min) * sample + min);

        Logger.Trace($"Calculated delay timeout: {value}ms.", new { min, max, distribution, sample });
        return value;
    }

    public int GetTimeout(TimeSpan timespan, DelayOptions? options = null) => GetTimeout((int)timespan.TotalMilliseconds, options);

    public int GetTimeout(TimeSpanRange range, DelayOptions? options = null) => GetTimeout((int)range.Start.TotalMilliseconds, (int)range.End.TotalMilliseconds, options);
}
