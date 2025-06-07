using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros;

public class BasicMacro : MacroBase
{
    public bool ShowStatusBar { get; set; }

    public Action<IConfigurableProcessor>? Configure { get; set; }

    public Func<bool>? Validate { get; set; }

    public Action<BasicMacroExecutionArguments>? Execute;

    public Func<BasicMacroExecutionArguments, Task>? ExecuteAsync;

    public BasicMacro() : base()
    {
    }

    public BasicMacro(string name) : base(name)
    {
    }

    protected override void OnConfigure(IConfigurableProcessor processor)
    {
        base.OnConfigure(processor);

        Configure?.Invoke(processor);

        processor.Services.AddTransient<BasicMacroExecutionArguments>();
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        if (ShowStatusBar)
        {
            InstallStatusBar(processor);
        }

        var loopService = processor.GetService<LoopService>();
        var hookService = processor.GetService<HookService>();

        if (Execute is not null)
        {
            processor.AddStep(new("execution", () =>
            {
                var args = processor.GetService<BasicMacroExecutionArguments>();
                Execute(args);
            })
            {
                IsDefault = true,
                IsInterruptable = true,
            });
        }
        else if (ExecuteAsync is not null)
        {
            processor.AddStep(new("execution", () =>
            {
                var args = processor.GetService<BasicMacroExecutionArguments>();
                ExecuteAsync(args).GetAwaiter().GetResult();
            })
            {
                IsDefault = true,
            });
        }

    }

    private static void InstallStatusBar(IPreparableProcessor processor)
    {
        var ph = processor.GetService<ProgressListInstrument>();
        ph.Title = "Status:";
        ph.Add(new(ProgressStatus.Idle)
        {
            Text = "Idle",
        });
        processor.GetService<DashboardService>().Add(ph);

        processor.Hooks.Register<ProcessorStartedHook>(_ =>
        {
            ph.Update(0, new(ProgressStatus.Busy)
            {
                Text = "Running",
            });
        });

        processor.Hooks.Register<ProcessorEndingHook>(e =>
        {
            var status = e.Reason switch
            {
                EndReason.Complete => ProgressStatus.Success,
                EndReason.Interrupted => ProgressStatus.Warning,
                EndReason.ErrorOccurred => ProgressStatus.Failure,
                _ => ProgressStatus.Idle,
            };
            ph.Update(0, new(status)
            {
                Text = e.Reason.ToString(),
            });
        });
    }

    protected override bool OnValidating([MaybeNullWhen(false)] out string invalidationMessage)
    {
        if (!base.OnValidating(out invalidationMessage))
        {
            return false;
        }

        if (Validate?.Invoke() == false)
        {
            invalidationMessage = ResourceHelper.Localize("Poltergeist.Automations/Resources/Validation_Failed");
            return false;
        }

        if (Execute is null && ExecuteAsync is null)
        {
            invalidationMessage = ResourceHelper.Localize("Poltergeist.Automations/Resources/Validation_BasicMacro_EmptyExecutions", nameof(BasicMacro), nameof(Execute), nameof(ExecuteAsync));
            return false;
        }
        
        return true;
    }
}
