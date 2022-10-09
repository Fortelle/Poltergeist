using System;
using System.Threading;
using System.Threading.Tasks;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Processors.Events;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Processors;

public class WorkingService : MacroService
{
    public Func<EndReason> WorkProc;
    public event EventHandler<BeginningEventArgs> Beginning;
    public event EventHandler<EndingEventArgs> Ending;

    public Func<Task> WorkingAsync;
    public Task WorkingTask;

    private readonly HookService Hooks;
    private bool CanAbort;

    private Thread WorkingThread;
    private CancellationTokenSource Cancellation;

    public bool WaitUI;
    private EndReason? EndStatus;

    public WorkingService(MacroProcessor processor, HookService hooks) : base(processor)
    {
        Hooks = hooks;
    }

    public void Start()
    {
        Cancellation = new();
        var start = new ThreadStart(ThreadProc);
        WorkingThread = new Thread(start);
        WorkingThread.SetApartmentState(ApartmentState.STA);
        WorkingThread.Start();
    }

    private void ThreadProc()
    {
        Hooks.Raise("process_starting");

        CheckInitializationError();
        if (EndStatus.HasValue) goto end;

        LoadServices();
        if (EndStatus.HasValue) goto end;

        CheckAvailability();
        if (EndStatus.HasValue) goto end;

        Hooks.Raise("process_started");

        DoWork();

        end:
        DoEnd();
    }

    public void Abort()
    {
        if (!CanAbort) return;

        Logger.Info("User aborted.");
        WorkingThread.Interrupt();
    }

    public void CheckCancel()
    {
        Cancellation.Token.ThrowIfCancellationRequested();
        //if (Cancellation.IsCancellationRequested)
        //{
        //    throw new Exception("User cancel");
        //}
    }

    private void CheckInitializationError()
    {
        if (Processor.InitializationException == null) return;
        Logger.Debug("An error has occurred during initialization.");

        DoError(Processor.InitializationException);
        EndStatus = EndReason.ErrorOccurred;
    }

    private void LoadServices()
    {
        Logger.Debug("Started loading startup services.");

        try
        {
            foreach (var type in Processor.AutoloadTypes)
            {
                Processor.GetService(type);
                Logger.Debug($"Loaded service <{type.Name}>.");
            }
        }
        catch (Exception e) //when (!Debugger.IsAttached)
        {
            DoError(e);
        }

        Logger.Debug("Finished loading startup services.");
    }

    // todo: add hooks if necessary
    private void CheckAvailability()
    {
        Logger.Debug("Started running availability check.");

        try
        {
            var isSucceeded = true;
            if (Beginning is not null)
            {
                var checkingList = Beginning.GetInvocationList();
                foreach (var checking in checkingList)
                {
                    var args = new BeginningEventArgs();
                    checking.DynamicInvoke(this, args);
                    if (args.Succeeded == false)
                    {
                        isSucceeded = false;
                        break;
                    }
                }
            }

            if (isSucceeded)
            {
                Logger.Debug("The availability check passed.");
            }
            else
            {
                Logger.Warn("The availability check failed."); // not an error
                EndStatus = EndReason.Unstarted;
            }

        }
        catch (Exception e) // when (!Debugger.IsAttached)
        {
            DoError(e);
        }

        Logger.Debug("Finished running availability check.");
    }

    private void DoWork()
    {
        Logger.Debug("Started running the main process.");

        try
        {
            CanAbort = true;

            EndStatus = WorkProc?.Invoke();
        }
        catch (ThreadInterruptedException e)
        {
            EndStatus = EndReason.UserAborted;
        }
        catch (MacroRunningException e)
        {
            DoError(e);
        }
        catch (Exception e) //when (!Debugger.IsAttached)
        {
            DoError(e);
        }
        finally
        {
            CanAbort = false;
        }

        Logger.Debug("Finished running the main process.");
    }

    private void DoError(Exception exception)
    {
        EndStatus = EndReason.ErrorOccurred;

        Logger.Error(exception.Message);

        if (exception.InnerException != null)
        {
            Logger.Error(exception.InnerException.Message);
        }

        Hooks.Raise("error_occured", exception.Message);
    }

    private void DoEnd()
    {
        if(!EndStatus.HasValue || EndStatus == EndReason.None)
        {
            EndStatus = EndReason.Complete;
        }

        Hooks.Raise("process_exiting", EndStatus.Value);

        Logger.Info($"The macro is finished with status: {EndStatus}.");

        Logger.Debug("The processor will shut down."); // this should be the last line

        if (WaitUI)
        {
            Thread.Sleep(100);
        }

        var args = new EndingEventArgs
        {
            Reason = EndStatus.Value
        };
        Ending?.Invoke(this, args);
    }

}
