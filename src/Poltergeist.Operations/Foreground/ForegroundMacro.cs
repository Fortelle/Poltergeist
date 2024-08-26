using System.Diagnostics;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Foreground;

// todo: close process
public class ForegroundMacro : MacroBase
{
    public LoopOptions LoopOptions { get; } = new()
    {
        IsCountLimitable = true,
        IsDurationLimitable = true,
        Instrument = LoopInstrumentType.List,
    };

    public RegionConfig? RegionConfig { get; set; }

    public string? Filename { get; set; }
    public string? Arguments { get; set; }
    public int Delay { get; set; }

    public Action<LoopBeforeArguments, ForegroundOperator>? Begin;
    public Action<LoopExecuteArguments, ForegroundOperator>? Iterate;
    public Action<LoopCheckContinueArguments, ForegroundOperator>? CheckContinue;
    public Action<ArgumentService, ForegroundOperator>? End;

    public ForegroundMacro(string name) : base(name)
    {
        Modules.Add(new InputOptionsModule());
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new ForegroundModule());
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        var repeat = processor.GetService<LoopService>();

        repeat.Before = OnBefore;

        if (Iterate is not null)
        {
            repeat.Execute = (e) => Iterate.Invoke(e, e.Processor.GetService<ForegroundOperator>());
        }

        if (CheckContinue is not null)
        {
            repeat.CheckContinue = (e) => CheckContinue.Invoke(e, e.Processor.GetService<ForegroundOperator>());
        }

        if (End is not null)
        {
            repeat.After = (e) => End.Invoke(e, e.Processor.GetService<ForegroundOperator>());
        }
    }

    private void OnBefore(LoopBeforeArguments e)
    {
        if (!string.IsNullOrEmpty(Filename))
        {
            var oldprocess = WindowsFinder.GetForegroundWindow();

            using var process = new Process()
            {
                StartInfo =
                {
                    FileName = Filename,
                    Arguments = Arguments,
                },
            };

            try
            {
                process.Start();
            }
            catch (Exception)
            {
                e.Cancel = true;
                return;
            }

            if (Delay > 0)
            {
                Thread.Sleep(Delay);
            }

            if (RegionConfig is null)
            {
                var newprocess = nint.Zero;
                while (newprocess == nint.Zero || oldprocess == newprocess)
                {
                    Thread.Sleep(100);
                    newprocess = WindowsFinder.GetForegroundWindow();
                }

                RegionConfig = new()
                {
                    Handle = newprocess,
                };
            }
        }

        RegionConfig ??= new();

        var foregroundOperator = e.Processor.GetService<ForegroundOperator>();
        var result = foregroundOperator.Locating.Locate(RegionConfig);
        if (!result)
        {
            e.Cancel = true;
            return;
        }

        Begin?.Invoke(e, foregroundOperator);
    }
}
