using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors.Events;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Processors;

public class WorkingService : KernelService
{
    public Func<EndReason>? WorkProc;
    private Thread? WorkingThread;

    public Func<Task<EndReason>>? AsyncWorkProc;
    private Task? WorkingTask;

    public event EventHandler<WorkBeginningEventArgs>? Beginning;
    public event EventHandler<WorkEndingEventArgs>? Ending;
    public event EventHandler? Ended;

    private readonly HookService Hooks;
    private readonly MacroLogger Logger;
    private bool CanAbort;

    public CancellationTokenSource? Cancellation;

    private EndReason? EndStatus;

    public bool IsAborted { get; set; }

    public WorkingService(MacroProcessor processor, HookService hooks, MacroLogger logger) : base(processor)
    {
        Hooks = hooks;
        Logger = logger;
    }

    public void Start()
    {
        if(WorkProc == null && AsyncWorkProc == null)
        {
            WorkProc = () => EndReason.Complete;
        }

        if(WorkProc != null)
        {
            var start = new ThreadStart(ThreadProc);
            WorkingThread = new Thread(start);
            WorkingThread.SetApartmentState(ApartmentState.STA);
            WorkingThread.Start();
        }
        else if (AsyncWorkProc != null)
        {
            WorkingTask = TaskProc();
        }
    }

    private void ThreadProc()
    {
        Hooks.Raise(new ProcessStartingHook());

        if (!DoInitialize())
        {
            DoEnd();
            return;
        }

        RaiseStartedEvent();

        Hooks.Raise(new ProcessStartedHook());

        DoWork();

        DoEnd();
    }

    private async Task TaskProc()
    {
        Cancellation = new();

        Hooks.Raise(new ProcessStartingHook());

        if (!DoInitialize())
        {
            DoEnd();
            return;
        }

        RaiseStartedEvent();

        Hooks.Raise(new ProcessStartedHook());

        await DoWorkAsync();

        DoEnd();
    }

    public void Abort()
    {
        if (!CanAbort)
        {
            return;
        }

        IsAborted = true;

        if (WorkingThread != null)
        {
            WorkingThread.Interrupt();
        }
        //else if (AsyncWorkProc != null)
        //{
        //    Cancellation?.Cancel();
        //}

        Cancellation?.Cancel();

        Logger.Log(LogLevel.Debug, ServiceName, "User aborted.");
    }

    public void CheckCancel()
    {
        Cancellation?.Token.ThrowIfCancellationRequested();
    }

    public CancellationToken GetCancellationToken()
    {
        Cancellation ??= new();
        return Cancellation.Token;
    }

    private void CheckInitializationError()
    {
        if (Processor.InitializationException == null)
        {
            return;
        }

        Logger.Log(LogLevel.Information, ServiceName, "An error has occurred during initialization.");

        DoError(Processor.InitializationException);
        EndStatus = EndReason.ErrorOccurred;
    }

    private void LoadServices()
    {
        Logger.Log(LogLevel.Debug, ServiceName, "Started loading startup services.");

        try
        {
            foreach (var serviceDescriptor in Processor.ServiceCollection!)
            {
                var serviceType = serviceDescriptor.ServiceType;
                var isAutoloadable = serviceType.IsAssignableTo(typeof(IAutoloadable));
                if (isAutoloadable)
                {
                    Processor.GetService(serviceType);
                    Logger.Log(LogLevel.Debug, ServiceName, $"Loaded service <{serviceType.Name}>.");
                }
            }
        }
        catch (Exception e) //when (!Debugger.IsAttached)
        {
            DoError(e);
        }

        Logger.Log(LogLevel.Debug, ServiceName, "Finished loading startup services.");
    }

    private void CheckAvailability()
    {
        Logger.Log(LogLevel.Debug, ServiceName, "Started running the availability check.");

        try
        {
            var isSucceeded = true;
            if (Beginning is not null)
            {
                var checkingList = Beginning.GetInvocationList();
                foreach (var checking in checkingList)
                {
                    var args = new WorkBeginningEventArgs();
                    checking.DynamicInvoke(this, args);
                    if (args.IsSucceeded == false)
                    {
                        isSucceeded = false;
                        break;
                    }
                }
            }

            if (isSucceeded)
            {
                Logger.Log(LogLevel.Debug, ServiceName, "The availability check passed.");
            }
            else
            {
                Logger.Log(LogLevel.Information, ServiceName, "The availability check failed."); // not an error
                EndStatus = EndReason.Unstarted;
            }

        }
        catch (Exception e) // when (!Debugger.IsAttached)
        {
            DoError(e);
        }

        Logger.Log(LogLevel.Debug, ServiceName, "Finished running the availability check.");
    }

    private bool DoInitialize()
    {
        CheckInitializationError();
        if (EndStatus.HasValue)
        {
            return false;
        }

        LoadServices();
        if (EndStatus.HasValue)
        {
            return false;
        }

        CheckAvailability();
        if (EndStatus.HasValue)
        {
            return false;
        }

        return true;
    }

    private void DoWork()
    {
        Logger.Log(LogLevel.Debug, ServiceName, "Started running the main process.");

        try
        {
            CanAbort = true;

            EndStatus = WorkProc?.Invoke();
        }
        catch (UserAbortException)
        {
            EndStatus = EndReason.UserAborted;
        }
        catch (ThreadInterruptedException) when (IsAborted)
        {
            EndStatus = EndReason.UserAborted;
        }
        catch (AggregateException e) when (e.InnerException is ThreadInterruptedException && IsAborted)
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

        Logger.Log(LogLevel.Debug, ServiceName, "Finished running the main process.");
    }

    private async Task DoWorkAsync()
    {
        Logger.Log(LogLevel.Debug, ServiceName, "Started running the main process.");

        try
        {
            CanAbort = true;

            EndStatus = await AsyncWorkProc!();
        }
        catch (UserAbortException)
        {
            EndStatus = EndReason.UserAborted;
        }
        catch (ThreadInterruptedException)
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

        Logger.Log(LogLevel.Debug, ServiceName, "Finished running the main process.");
    }


    private void DoError(Exception exception)
    {
        EndStatus = EndReason.ErrorOccurred;

        Logger.Log(LogLevel.Error, ServiceName, exception.Message);

        if (exception.InnerException != null)
        {
            Logger.Log(LogLevel.Error, ServiceName, exception.InnerException.Message);
        }

        Hooks.Raise(new ErrorOccurredHook(exception.Message));
    }

    private void DoEnd()
    {
        if(!EndStatus.HasValue || EndStatus == EndReason.None)
        {
            EndStatus = EndReason.Complete;
        }

        try
        {
            Hooks.Raise(new ProcessExitingHook(EndStatus.Value));
        }
        catch (Exception exception)
        {
            EndStatus = EndReason.ErrorOccurred;

            Logger.Log(LogLevel.Error, ServiceName, exception.Message);
        }

        try
        {
            var args = new WorkEndingEventArgs
            {
                Reason = EndStatus.Value
            };
            Ending?.Invoke(this, args);
        }
        catch (Exception exception)
        {
            EndStatus = EndReason.ErrorOccurred;
            Logger.Log(LogLevel.Error, ServiceName, exception.Message);
        }

        Logger.Log(LogLevel.Information, ServiceName, $"The macro has ended: {EndStatus}.");

        Logger.Log(LogLevel.Debug, ServiceName, "The processor will shut down."); // this should be the last line

        try
        {
            Ended?.Invoke(this, new());
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private void RaiseStartedEvent()
    {
        var startedActions = new List<StartedAction>();
        if (Processor.Macro.MinimizeApplication)
        {
            startedActions.Add(StartedAction.MinimizedWindow);
        }

        Processor.RaiseEvent(MacroEventType.ProcessStarted, new MacroStartedEventArgs()
        {
            Started = !EndStatus.HasValue,
            StartedActions = startedActions.ToArray(),
        });
    }

}
