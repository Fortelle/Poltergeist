using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors.Events;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Processors;

public class WorkingService : MacroService
{
    public Action WorkProc;
    public event EventHandler<BeginningEventArgs> Beginning;
    public event EventHandler<EndingEventArgs> Ending;

    public Func<Task> WorkingAsync;
    public Task WorkingTask;

    private readonly HookService Hooks;
    private bool CanAbort;

    private Thread WorkingThread;
    private CancellationTokenSource Cancellation;

    public bool WaitUI;

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

        if (!CheckInitializationError()) return;
        if (!LoadServices()) return;
        if (!CheckAvailability()) return;

        Hooks.Raise("process_started");

        if (!DoWork()) return;

        DoEnd(ProcessEndReason.Completed);
    }

    public void Abort()
    {
        if (!CanAbort) return;

        Log(LogLevel.Information, "User aborted.");
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

    private bool CheckInitializationError()
    {
        if (Processor.InitializationException == null) return true;

        Log(LogLevel.Debug, "An error has occurred during initialization.");

        DoError(Processor.InitializationException);
        DoEnd(ProcessEndReason.ErrorOccurred);
        return false;
    }

    private bool LoadServices()
    {
        ProcessEndReason? endReason = null;

        Log(LogLevel.Debug, "Started loading startup services.");

        try
        {
            foreach (var type in Processor.AutoloadTypes)
            {
                Processor.GetService(type);
                Log(LogLevel.Debug, $"Loaded service <{type.Name}>.");
            }
        }
        catch (Exception e) //when (!Debugger.IsAttached)
        {
            DoError(e);
            endReason = ProcessEndReason.ErrorOccurred;
        }

        Log(LogLevel.Debug, "Finished loading startup services.");

        if (endReason.HasValue)
        {
            DoEnd(endReason.Value);
            return false;
        }
        else
        {
            return true;
        }
    }

    // todo: add hooks if necessary
    private bool CheckAvailability()
    {
        ProcessEndReason? endReason = null;

        Log(LogLevel.Debug, "Started running availability check.");

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
                Log(LogLevel.Debug, "The availability check passed.");
            }
            else
            {
                Log(LogLevel.Warning, "The availability check failed."); // not an error
                endReason = ProcessEndReason.CheckFailed;
            }

        }
        catch (Exception e) // when (!Debugger.IsAttached)
        {
            DoError(e);
            endReason = ProcessEndReason.ErrorOccurred;
        }

        Log(LogLevel.Debug, "Finished running availability check.");

        if (endReason.HasValue)
        {
            DoEnd(endReason.Value);
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool DoWork()
    {
        ProcessEndReason? endReason = null;

        Log(LogLevel.Debug, "Started running the main process.");

        try
        {
            CanAbort = true;

            Hooks.Raise("process_begin");

            WorkProc?.Invoke();

            Hooks.Raise("process_done");
        }
        catch (ThreadInterruptedException e)
        {
            endReason = ProcessEndReason.UserAborted;
        }
        catch (MacroRunningException e)
        {
            DoError(e);
            endReason = ProcessEndReason.ErrorOccurred;
        }
        catch (Exception e) //when (!Debugger.IsAttached)
        {
            DoError(e);
            endReason = ProcessEndReason.ErrorOccurred;
        }
        finally
        {
            CanAbort = false;
        }

        Log(LogLevel.Debug, "Finished running the main process.");

        if (endReason.HasValue)
        {
            DoEnd(endReason.Value);
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DoError(Exception exception)
    {
        Log(LogLevel.Error, exception.Message);

        if (exception.InnerException != null)
        {
            Log(LogLevel.Error, exception.InnerException.Message);
        }

        Hooks.Raise("error_occured", exception.Message);
    }

    private void DoEnd(ProcessEndReason reason)
    {
        Hooks.Raise("process_exiting", reason);

        Log(LogLevel.Information, "The macro is complete. The processor will shut down."); // this should be the last line

        if (WaitUI)
        {
            Thread.Sleep(100);
        }

        var args = new EndingEventArgs
        {
            Reason = reason
        };
        Ending?.Invoke(this, args);
    }

}
