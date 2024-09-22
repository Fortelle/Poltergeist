using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    public bool IsCancelled { get; set; }

    private PauseProvider? PauseProvider;

    private CancellationTokenSource? Cancellation { get; set; }
    CancellationToken? IUserProcessor.CancellationToken => Cancellation?.Token;

    private bool CanAbort { get; set; }

    public async Task Pause()
    {
        Log(LogLevel.Debug, "The processor is paused.");

        PauseProvider = new();
        await PauseProvider.Pause();
        PauseProvider = null;

        if (IsCancelled)
        {
            throw new UserAbortException();
        }
    }

    public void Resume()
    {
        if (PauseProvider is null)
        {
            return;
        }

        Log(LogLevel.Debug, "The processor is resumed.");

        PauseProvider.Resume();
    }

    public void CheckCancel()
    {
        Cancellation?.Token.ThrowIfCancellationRequested();
    }

    public void Abort() // todo: add reason
    {
        IsCancelled = true;

        if (!CanAbort)
        {
            return;
        }

        Log(LogLevel.Information, "User aborting.");

        WorkingThread?.Interrupt();

        Cancellation?.Cancel();

        if (PauseProvider?.IsPaused == true)
        {
            PauseProvider.Resume();
        }
    }

}

