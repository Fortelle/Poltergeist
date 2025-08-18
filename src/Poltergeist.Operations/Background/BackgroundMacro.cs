using System.Diagnostics;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Background;

public class BackgroundMacro : CommonMacroBase
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

    public Action<LoopBeforeArguments, BackgroundOperator>? Begin;
    public Action<IterationArguments, BackgroundOperator>? Iterate;
    public Action<LoopCheckContinueArguments, BackgroundOperator>? CheckContinue;
    public Action<ArgumentService, BackgroundOperator>? End;

    public BackgroundMacro(string name) : base(name)
    {
        Modules.Add(new InputOptionsModule());
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new BackgroundModule());
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        var loopService = processor.GetService<LoopService>();
        var hookService = processor.GetService<HookService>();

        hookService.Register<LoopStartedHook>(hook =>
        {
            var args = hook.Processor.GetService<LoopBeforeArguments>();
            OnBefore(args);
        });

        if (Iterate is not null)
        {
            hookService.Register<IterationExecutingHook>(hook =>
            {
                var args = hook.Processor.GetService<IterationArguments>();
                args.Index = hook.Index;
                args.Result = IterationResult.Continue;

                Iterate.Invoke(args, hook.Processor.GetService<BackgroundOperator>());

                hook.Result = args.Result;
            });
        }

        if (CheckContinue is not null)
        {
            hookService.Register<LoopCheckContinueHook>(hook =>
            {
                var args = hook.Processor.GetService<LoopCheckContinueArguments>();
                args.IterationIndex = hook.IterationIndex;
                args.IterationResult = hook.IterationResult;
                CheckContinue.Invoke(args, hook.Processor.GetService<BackgroundOperator>());
                hook.Result = args.Result;
            });
        }

        if (End is not null)
        {
            hookService.Register<LoopCheckContinueHook>(hook =>
            {
                var args = hook.Processor.GetService<ArgumentService>();
                End.Invoke(args, hook.Processor.GetService<BackgroundOperator>());
            });
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

        if (RegionConfig is null)
        {
            e.Cancel = true;
            return;
        }

        var backgroundOperator = e.Processor.GetService<BackgroundOperator>();
        var result = backgroundOperator.Locating.Locate(RegionConfig);
        if (!result)
        {
            e.Cancel = true;
            return;
        }
        Begin?.Invoke(e, backgroundOperator);
    }
}
