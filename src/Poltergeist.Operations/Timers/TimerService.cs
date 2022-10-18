using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Maths;

namespace Poltergeist.Operations.Timers;

public class TimerService : MacroService
{
    private DelayOptions DefaultOptions { get; }
    private DistributionService Distribution { get; }
    private WorkingService Workflow { get; }

    public TimerService(
        MacroProcessor processor,
        DistributionService random,
        WorkingService workflow,
        IOptions<DelayOptions> options
        )
        : base(processor)
    {
        Workflow = workflow;
        Distribution = random;
        DefaultOptions = options.Value;

        Logger.Debug($"Initialized <{nameof(TimerService)}>.", DefaultOptions);
    }


    #region "Delay"

    public void Delay(int milliseconds, DelayOptions options = null)
    {
        //Logger.Debug($"Delaying for {milliseconds} ms.", options);

        var timeout = GetTimeout(milliseconds, options);
        DoDelay(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { milliseconds, options });
    }

    public void Delay(int min, int max, DelayOptions options = null)
    {
        //Logger.Debug($"Delaying for {min}-{max} ms.", options);

        var timeout = GetTimeout(min, max, options);
        DoDelay(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { min, max, options });
    }

    public async Task DelayAsync(int milliseconds, DelayOptions options = null)
    {
        //Logger.Debug($"Delaying for {milliseconds} ms.", options);

        var timeout = GetTimeout(milliseconds, options);
        await DoDelayAsync(timeout);

        Logger.Debug($"Delayed for {timeout} ms.", new { milliseconds, options });
    }

    public async Task DelayAsync(int min, int max, DelayOptions options = null)
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
            System.Threading.Thread.Sleep(timeout);
        }
        //Logger.Debug($"Delayed for {timeout} ms.");

        Workflow.CheckCancel();
    }

    private async Task DoDelayAsync(int timeout)
    {
        if (timeout > 0)
        {
            await Task.Delay(timeout);
        }
        //Logger.Debug($"Delayed for {timeout} ms.");

        Workflow.CheckCancel();
    }

    private int GetTimeout(int milliseconds, DelayOptions options)
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

    private int GetTimeout(int min, int max, DelayOptions options)
    {
        var distribution = options?.RangeDistribution ?? DefaultOptions?.RangeDistribution ?? RangeDistributionType.Uniform;

        var sample = Distribution.NextDouble(distribution);
        var value = (max - min) * sample + min;
        return (int)value;
    }

}
