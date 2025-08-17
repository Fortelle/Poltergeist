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
    /// <remarks>
    /// You can subscribe to the <see cref="Completed"/> event to get notified when the processor is completed.
    /// </remarks>
    public void Start()
    {
        if (Status != ProcessorStatus.Idle)
        {
            throw new InvalidOperationException();
        }

        InternalStart();
    }

    /// <summary>
    /// Executes the processor synchronously. Blocks the current thread until the processor is completed.
    /// </summary>
    public ProcessorResult Execute()
    {
        if (Status != ProcessorStatus.Idle)
        {
            throw new InvalidOperationException();
        }

        using var mre = new ManualResetEvent(false);

        Completed += (_, _) =>
        {
            mre.Set();
        };
        
        InternalExecute();

        mre.WaitOne();

        return Result!;
    }

    /// <summary>
    /// Executes the processor asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task<ProcessorResult> ExecuteAsync()
    {
        if (Status != ProcessorStatus.Idle)
        {
            throw new InvalidOperationException();
        }

        var tcs = new TaskCompletionSource();

        Completed += (_, _) =>
        {
            tcs.SetResult();
        };

        InternalExecute();

        await tcs.Task.ConfigureAwait(false);

        return Result!;
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
            PauseReason.Manual => "The macro is paused by the user.",
            PauseReason.WaitForInput => "The macro is paused for user input.",
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

        Cancellation?.Cancel();

        if (CanInterrupt)
        {
            WorkflowThread?.Interrupt();
        }

        IsCancelled = true;
    }

    /// <summary>
    /// Forces to terminate the processor that is run via the <see cref="Start"/> method.
    /// </summary>
    /// <remarks>
    /// This method terminates the processor brutally. Beware that the resources are not guaranteed to be released.
    /// </remarks>
    public void Terminate()
    {
        if (ProcessThread is null)
        {
            throw new InvalidOperationException("The processor is not running.");
        }

        Logger?.Trace("Received a termination request.");

        Status = ProcessorStatus.Terminating;

        WorkflowThread?.Interrupt();
        ProcessThread.Interrupt();
    }
}
