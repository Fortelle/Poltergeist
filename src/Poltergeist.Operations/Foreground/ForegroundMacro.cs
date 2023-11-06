using System.Diagnostics;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Input.Windows;

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

    private ForegroundOperator? Operator { get; set; }

    public RegionConfig RegionConfig { get; set; }

    public string? Filename { get; set; }
    public string? Arguments { get; set; }

    public Action<LoopBeforeArguments, ForegroundOperator>? Begin;
    public Action<LoopExecutionArguments, ForegroundOperator>? Iteration;
    public Action<LoopCheckContinueArguments, ForegroundOperator>? CheckContinue;
    public Action<ArgumentService, ForegroundOperator>? End;

    public ForegroundMacro(string name) : base(name)
    {
    }

    protected override void OnInitialize()
    {
        Modules.Add(new InputOptionsModule());
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new ForegroundModule());
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var repeat = processor.GetService<LoopService>();
        repeat.Before = OnBegin;
        if (Iteration != null)
        {
            repeat.Execution = (e) => Iteration.Invoke(e, Operator!);
        }

        if (CheckContinue != null)
        {
            repeat.CheckContinue = (e) => CheckContinue.Invoke(e, Operator!);
        }

        if (End != null)
        {
            repeat.After = (e) => End.Invoke(e, Operator!);
        }
    }

    private void OnBegin(LoopBeforeArguments e)
    {
        Operator = e.Processor.GetService<ForegroundOperator>();

        if (!string.IsNullOrEmpty(Filename))
        {
            var oldprocess = WindowsFinder.GetForegroundWindow();

            using var process = new Process()
            {
                StartInfo =
                {
                    FileName = Filename,
                    Arguments = Arguments,
                    UseShellExecute = true,
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

            var newprocess = nint.Zero;
            while (newprocess == nint.Zero || oldprocess == newprocess)
            {
                Thread.Sleep(100);
                newprocess = WindowsFinder.GetForegroundWindow();
            }

            RegionConfig = RegionConfig with
            {
                Handle = newprocess
            };
        }

        var result = Operator.Locating.Locate(RegionConfig);
        if (!result)
        {
            e.Cancel = true;
            return;
        }
        Begin?.Invoke(e, Operator);
    }
}
