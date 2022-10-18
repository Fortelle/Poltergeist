using System;
using System.Diagnostics;
using System.Threading;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Components.Loops;
using Poltergeist.Operations.ForegroundWindows;

namespace Poltergeist.Operations.Macros;

// todo: close process
public class ForegroundMacro : MacroBase
{
    private ForegroundOperator Operator { get; set; }

    public RegionConfig RegionConfig { get; set; }

    public string Filename { get; set; }
    public string Arguments { get; set; }

    public Action<LoopBeginArguments, ForegroundOperator> Begin;
    public Action<LoopIterationArguments, ForegroundOperator> Iteration;
    public Action<LoopCheckNextArguments, ForegroundOperator> CheckNext;
    public Action<ArgumentService, ForegroundOperator> End;

    public ForegroundMacro(string name) : base(name)
    {
        Modules.Add(new InputOptionsModule());
        Modules.Add(new RepeatModule());
        Modules.Add(new ForegroundModule());
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var repeat = processor.GetService<RepeatService>();
        repeat.BeginProc = OnBegin;
        if (Iteration != null) repeat.IterationProc = (e) => Iteration.Invoke(e, Operator);
        if (CheckNext != null) repeat.CheckNextProc = (e) => CheckNext.Invoke(e, Operator);
        if (End != null) repeat.EndProc = (e) => End.Invoke(e, Operator);
    }

    private void OnBegin(LoopBeginArguments e)
    {
        Operator = e.Processor.GetService<ForegroundOperator>();

        if (!string.IsNullOrEmpty(Filename))
        {
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

            do
            {
                Thread.Sleep(100);
                process.Refresh();
            } while (process.MainWindowHandle == IntPtr.Zero);

            RegionConfig = RegionConfig with
            {
                Handle = process.MainWindowHandle
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
