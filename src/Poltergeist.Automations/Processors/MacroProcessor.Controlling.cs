namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    private PauseProvider? PauseProvider;

    private readonly CancellationTokenSource CancellationTokenSource = new ();

    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public bool IsCancellationRequested => CancellationTokenSource.IsCancellationRequested;

    public void ThrowIfCancellationRequested()
    {
        WorkflowCanceledException.ThrowIf(IsCancellationRequested);
    }
    
    /// <summary>
    /// Runs the processor in a new thread.
    /// </summary>
    /// <remarks>
    /// You can subscribe to the <see cref="Completed"/> event to get notified when the processor is completed.
    /// </remarks>
    public void Start()
    {
        CheckRunnable();

        WorkflowTask = Task.Run(ProcessAsync);
    }

    /// <summary>
    /// Executes the processor synchronously. Blocks the current thread until the processor is completed.
    /// </summary>
    public ProcessorResult Execute()
    {
        CheckRunnable();

        WorkflowTask = ProcessAsync();

        return WorkflowTask.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes the processor asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task<ProcessorResult> ExecuteAsync()
    {
        CheckRunnable();

        WorkflowTask = ProcessAsync();

        return await WorkflowTask;
    }

    /// <summary>
    /// Waits for the processor to complete and returns the result.
    /// </summary>
    /// <returns>The result of the processor.</returns>
    public ProcessorResult GetResult()
    {
        if (WorkflowTask is null)
        {
            throw new InvalidOperationException("The processor has not been started.");
        }

        return WorkflowTask.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Waits for the processor to complete and returns the result asynchronously.
    /// </summary>
    /// <returns>The result of the processor.</returns>
    public Task<ProcessorResult> GetResultAsync()
    {
        if (WorkflowTask is null)
        {
            throw new InvalidOperationException("The processor has not been started.");
        }

        return WorkflowTask;
    }

    /// <summary>
    /// Pauses the processor.
    /// </summary>
    /// <param name="reason"></param>
    public async Task Pause(PauseReason reason = PauseReason.Unknown)
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

        ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Tries to stop the processor.
    /// </summary>
    /// <param name="reason"></param>
    public void Stop(AbortReason reason = AbortReason.Unknown)
    {
        Logger?.Debug("Received a stop request.");

        if (Status != ProcessorStatus.Running)
        {
            return;
        }

        Status = ProcessorStatus.Stopping;

        CancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Forces to terminate the processor that is run via the <see cref="Start"/> method.
    /// </summary>
    /// <remarks>
    /// This method terminates the processor brutally. Beware that the resources are not guaranteed to be released.
    /// </remarks>
    public void Terminate()
    {
        throw new NotImplementedException();
    }

    private void CheckRunnable()
    {
        if (Macro.Exception is not null)
        {
            throw new InvalidOperationException("The macro is not able to run.", Macro.Exception);
        }

        if (Exception is not null)
        {
            throw new InvalidOperationException("The processor is not initialized correctly.", Exception);
        }

        if (Status != ProcessorStatus.Idle)
        {
            throw new InvalidOperationException("The processor is not idle.");
        }

        if (IsDisposed)
        {
            throw new InvalidOperationException("The processor is disposed.");
        }
    }
}
