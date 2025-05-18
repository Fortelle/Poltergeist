namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    public bool IsCancelled { get; set; }

    private PauseProvider? PauseProvider;

    private CancellationTokenSource? Cancellation;

    CancellationToken IUserProcessor.CancellationToken => Cancellation?.Token ?? CancellationToken.None;

    /// <summary>
    /// Runs the processor in a new thread.
    /// </summary>
    public void Run()
    {
        InternalRun();
    }

    /// <summary>
    /// Pauses the processor.
    /// </summary>
    /// <param name="reason"></param>
    public async Task Pause(PauseReason reason)
    {
        PauseProvider = new();

        Logger?.Debug(reason switch
        {
            PauseReason.User => "The macro is paused by the user.",
            PauseReason.Input => "The macro is paused for user input.",
            _ => "The macro is paused."
        });

        await PauseProvider.Pause();

        PauseProvider = null;

        if (IsCancelled)
        {
            throw new UserAbortException();
        }
    }

    public void Resume()
    {
        Logger?.Trace("Received a resume request.");

        if (PauseProvider is null)
        {
            Logger?.Trace($"The processor is not paused.");
        }
        else
        {
            PauseProvider.Resume();
            Logger?.Info("The macro is resumed.");
        }
    }

    public bool IsInterrupted()
    {
        return IsCancelled || Cancellation?.Token.IsCancellationRequested == true;
    }

    public void ThrowIfInterrupted()
    {
        if (IsInterrupted())
        {
            throw new WorkflowStoppedException();
        }
    }

    /// <summary>
    /// Tries to stop the processor.
    /// </summary>
    /// <param name="reason"></param>
    public void Stop(AbortReason reason)
    {
        Logger?.Debug("Received a stop request.");

        if (Status != ProcessorStatus.Running)
        {
            return;
        }

        Status = ProcessorStatus.Stopping;

        if (CanAbort)
        {
            Cancellation?.Cancel();

            WorkflowThread?.Interrupt();
        }
    }

    /// <summary>
    /// Forces to terminate the processor.
    /// </summary>
    public void Terminate()
    {
        Logger?.Trace("Received a termination request.");

        if (ProcessThread is null)
        {
            throw new Exception("The processor is not running.");
        }

        Status = ProcessorStatus.Terminating;

        WorkflowThread?.Interrupt();
        ProcessThread.Interrupt();
    }
}
