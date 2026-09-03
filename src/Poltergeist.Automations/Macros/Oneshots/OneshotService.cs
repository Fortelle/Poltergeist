using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Macros.Oneshots;

public class OneshotService : MacroService, IWorkflowExecutor
{
    private readonly HookService HookService;

    private Func<IMacroProcessorShared, Task>? ExecuteAsync;
    private Action<IMacroProcessorShared>? Finalize;

    public OneshotService(
        MacroProcessor processor,
        HookService hookService
        ) : base(processor)
    {
        HookService = hookService;
    }

    public async Task<bool> Process()
    {
        Setup();

        await InternalExecute();

        return true;
    }

    private void Setup()
    {
        Logger.Debug("Started running the oneshot-setup procedure.");


        Processor.ThrowIfCancellationRequested();

        var hook = new OneshotSetupHook();

        HookService.Raise(hook);

        ExecuteAsync = hook.ExecuteAsync;
        Finalize = hook.Finalize;


        Logger.Debug("Finished running the oneshot-setup procedure.");
    }

    private async Task InternalExecute()
    {
        Logger.Debug("Started running the execution procedure.");


        Processor.ThrowIfCancellationRequested();

        if (ExecuteAsync is not null)
        {
            await ExecuteAsync.Invoke(Processor);
        }


        Logger.Debug("Finished running the execution procedure.");
    }

    public void Cleanup()
    {
        Logger.Debug("Started running the cleanup procedure.");

        if (Finalize is not null)
        {
            Finalize.Invoke(Processor);
        }

        Logger.Debug("Finished running the cleanup procedure.");
    }
}
