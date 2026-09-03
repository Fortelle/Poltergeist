using Poltergeist.Automations.Macros.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Hybrid;
using Poltergeist.Operations.Inputting;

namespace Poltergeist.Android.HybridEmulators;

public class HybridAndroidEmulatorMacro : CommonLoopMacroBase
{
    public Action<WorkflowController>? BeforeConnect;
    public Action<WorkflowController, HybridOperator>? AfterConnect;
    public Action<WorkflowController, IterationContext, HybridOperator>? Execute;
    public Action<WorkflowController, TransitionContext, HybridOperator>? Transition;
    public Action<WorkflowController>? End;

    public HybridAndroidEmulatorMacro(string? name = null) : base(name)
    {
        Modules.Add(new HybridAndroidEmulatorModule());
        Modules.Add(new HumanizationOptionsModule());
        Modules.Add(new AdbHumanizationOptionsModule());
    }

    protected override Task<bool> OnStartAsync(WorkflowController controller)
    {
        BeforeConnect?.Invoke(controller);

        var emulatorService = controller.Processor.GetService<HybridOperationService>();
        emulatorService.Connect();

        if (AfterConnect is not null)
        {
            var ope = controller.Processor.GetService<HybridOperator>();
            AfterConnect.Invoke(controller, ope);
        }

        return Task.FromResult(true);
    }

    protected override Task OnExecuteAsync(WorkflowController controller, IterationContext context)
    {
        if (Execute is not null)
        {
            var @operator = controller.Processor.GetService<HybridOperator>();
            Execute.Invoke(controller, context, @operator);
        }

        return Task.CompletedTask;
    }

    protected override Task OnTransitionAsync(WorkflowController controller, TransitionContext context)
    {
        if (Transition is not null)
        {
            var @operator = controller.Processor.GetService<HybridOperator>();
            Transition.Invoke(controller, context, @operator);
        }

        return Task.CompletedTask;
    }

    protected override Task OnEndAsync(WorkflowController controller)
    {
        if (End is not null)
        {
            End.Invoke(controller);
        }

        return Task.CompletedTask;
    }
}
