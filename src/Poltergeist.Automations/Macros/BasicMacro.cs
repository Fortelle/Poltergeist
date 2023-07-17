using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public class BasicMacro : MacroBase
{
    public bool ShowStatusBar { get; set; }

    public Action<BasicMacroExecutionArguments>? Execution;

    public Func<BasicMacroExecutionArguments, Task>? AsyncExecution;

    public BasicMacro() : base()
    {
    }

    public BasicMacro(string name) : base(name)
    {
    }

    protected override void OnConfigure(ServiceCollection services, IConfigureProcessor processor)
    {
        base.OnConfigure(services, processor);

        services.AddTransient<BasicMacroExecutionArguments>();
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        if (ShowStatusBar)
        {
            InstallStatusBar(processor);
        }

        var work = processor.GetService<WorkingService>();
        if(Execution != null)
        {
            work.WorkProc = () =>
            {
                var args = processor.GetService<BasicMacroExecutionArguments>();
                Execution(args);
                return EndReason.Complete;
            };
        }
        else if (AsyncExecution != null)
        {
            work.AsyncWorkProc = async () =>
            {
                var args = processor.GetService<BasicMacroExecutionArguments>();
                await AsyncExecution(args);
                return EndReason.Complete;
            };
        }

    }

    private static void InstallStatusBar(MacroProcessor processor)
    {
        var ph = processor.GetService<ProgressListInstrument>();
        ph.Title = "Status:";
        ph.Add(new(ProgressStatus.Idle)
        {
            Text = "Idle",
        });
        processor.GetService<DashboardService>().Add(ph);

        var hooks = processor.GetService<HookService>();

        hooks.Register<ProcessStartedHook>(_ =>
        {
            ph.Update(0, new(ProgressStatus.Busy)
            {
                Text = "Running",
            });
        });

        hooks.Register<ProcessExitingHook>(e =>
        {
            var status = e.Reason switch
            {
                EndReason.Complete or EndReason.Purposed => ProgressStatus.Success,
                EndReason.Unstarted or EndReason.UserAborted => ProgressStatus.Warning,
                EndReason.ErrorOccurred => ProgressStatus.Failure,
                _ => ProgressStatus.Idle,
            };
            ph.Update(0, new(status)
            {
                Text = e.Reason.ToString(),
            });
        });
    }
}
