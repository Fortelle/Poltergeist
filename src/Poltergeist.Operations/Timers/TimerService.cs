﻿using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Timers;

public class TimerService : MacroService
{
    private readonly DelayOptions DefaultOptions;

    private readonly DistributionService Distribution;

    public TimerService(
        MacroProcessor processor,
        DistributionService distribution,
        IOptions<DelayOptions> options
        ) : base(processor)
    {
        Distribution = distribution;
        DefaultOptions = options.Value;
    }


    #region "Delay"

    public void Delay(int milliseconds, DelayOptions? options = null)
    {
        //Logger.Debug($"Delaying for {milliseconds} ms.", options);

        var timeout = GetTimeout(milliseconds, options);
        DoDelay(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { milliseconds, options });
    }

    public void Delay(int min, int max, DelayOptions? options = null)
    {
        //Logger.Debug($"Delaying for {min}-{max} ms.", options);

        var timeout = GetTimeout(min, max, options);
        DoDelay(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { min, max, options });
    }

    public async Task DelayAsync(int milliseconds, DelayOptions? options = null)
    {
        //Logger.Debug($"Delaying for {milliseconds} ms.", options);

        var timeout = GetTimeout(milliseconds, options);
        await DoDelayAsync(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { milliseconds, options });
    }

    public async Task DelayAsync(int min, int max, DelayOptions? options = null)
    {
        //Logger.Debug($"Delaying for {min}-{max} ms.", options);

        var timeout = GetTimeout(min, max, options);
        await DoDelayAsync(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { min, max, options });
    }

    #endregion

    private void DoDelay(int timeout)
    {
        if (timeout > 0)
        {
            Thread.Sleep(timeout);
        }

        Processor.ThrowIfInterrupted();
    }

    private async Task DoDelayAsync(int timeout)
    {
        if (timeout > 0)
        {
            await Task.Delay(timeout);
        }
        //Logger.Debug($"Delayed for {timeout} ms.");

        Processor.ThrowIfInterrupted();
    }

    private int GetTimeout(int milliseconds, DelayOptions? options)
    {
        var floating = options?.Floating ?? DefaultOptions?.Floating ?? false;
        if (!floating)
        {
            return milliseconds;
        }

        var (rateMin, rateMax) = options?.FloatingRange ?? DefaultOptions?.FloatingRange ?? (1, 1);
        var distribution = options?.FloatDistribution ?? DefaultOptions?.FloatDistribution ?? RangeDistributionType.Uniform;

        var min = milliseconds * rateMin;
        var max = milliseconds * rateMax;

        var sample = Distribution.NextDouble(distribution);
        var value = (max - min) * sample + min;
        return (int)value;
    }

    private int GetTimeout(int min, int max, DelayOptions? options)
    {
        var distribution = options?.RangeDistribution ?? DefaultOptions?.RangeDistribution ?? RangeDistributionType.Uniform;

        var sample = Distribution.NextDouble(distribution);
        var value = (max - min) * sample + min;
        return (int)value;
    }

}
