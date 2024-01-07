using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;

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

        if(Execute != null)
        {
            processor.WorkProc = () =>
            {
                var args = processor.GetService<BasicMacroExecutionArguments>();
                Execute(args);
            };
        }
        else if (ExecuteAsync != null)
        {
            processor.AsyncWorkProc = async () =>
            {
                var args = processor.GetService<BasicMacroExecutionArguments>();
                await ExecuteAsync(args);
            };
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
                EndReason.Complete or EndReason.UserAborted => ProgressStatus.Success,
                EndReason.Unstarted => ProgressStatus.Warning,
                EndReason.ErrorOccurred => ProgressStatus.Failure,
                _ => ProgressStatus.Idle,
            };
            ph.Update(0, new(status)
            {
                Text = e.Reason.ToString(),
            });
        });
    }

    public override string? CheckValidity()
    {
        if (base.CheckValidity() is string baseValue)
        {
            return baseValue;
        }

        if (Validate?.Invoke() == false)
        {
            return "Failed to validate.";
        }

        if (Execute is null && ExecuteAsync is null)
        {
            return ResourceHelper.Localize("Poltergeist.Automations/Resources/Validation_BasicMacro_EmptyExecutions", nameof(BasicMacro), nameof(Execute), nameof(ExecuteAsync));
        }
        
        return null;
    }
}
