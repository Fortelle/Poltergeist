using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public sealed partial class MacroProcessor : IFrontProcessor, IServiceProcessor, IConfigurableProcessor, IUserProcessor, IPreparableProcessor
{
    public string ProcessId { get; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public ServiceCollection? ServiceCollection { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }
    ServiceCollection IConfigurableProcessor.Services => ServiceCollection!;

    public ParameterValueCollection Options { get; } = new();
    public ParameterValueCollection Environments { get; } = new();
    public ParameterValueCollection Statistics { get; } = new();
    public ParameterValueCollection SessionStorage { get; } = new();

    public LaunchReason Reason { get; set; }

    public string? Comment { get; set; }

    public Exception? Exception { get; private set; }

    public ProcessorStatus Status { get; private set; } = ProcessorStatus.Idle;

    private IBackMacro Macro { get; }
    IUserMacro IUserProcessor.Macro => (IUserMacro)Macro;
    IUserMacro IPreparableProcessor.Macro => (IUserMacro)Macro;
    IFrontMacro IFrontProcessor.Macro => (IFrontMacro)Macro;

    private readonly SynchronizationContext? OriginalContext = SynchronizationContext.Current;

    private LoggerWrapper? Logger;

    public MacroProcessor(MacroBase macro)
    {
        Macro = macro;
        ProcessId = Guid.NewGuid().ToString();

        Macro.Initialize();

        var invalidationMessage = Macro.CheckValidity();
        if (invalidationMessage is not null)
        {
            Exception = new Exception(invalidationMessage);
            Status = ProcessorStatus.Invalid;
            return;
        }
    }

    public MacroProcessor(MacroBase macro, MacroProcessorArguments arguments) : this(macro)
    {
        Reason = arguments.LaunchReason;

        if (arguments.Options?.Count > 0)
        {
            foreach (var (key, value) in arguments.Options)
            {
                Options.Reset(key, value);
            }
        }

        if (arguments.Environments?.Count > 0)
        {
            foreach (var (key, value) in arguments.Environments)
            {
                Environments.Reset(key, value);
            }
        }
        if (arguments.SessionStorage?.Count > 0)
        {
            foreach (var (key, value) in arguments.SessionStorage)
            {
                SessionStorage.Reset(key, value);
            }
        }

        if (arguments.Statistics is not null)
        {
            foreach (var (key, value) in arguments.Statistics)
            {
                Statistics.Reset(key, value);
            }
        }
    }

    public T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T))!;
    }

    public object? GetService(Type type)
    {
        return ServiceProvider!.GetService(type);
    }

    public void ReceiveMessage(Dictionary<string, string> paramaters)
    {
        GetService<HookService>().Raise(new MessageReceivedHook(paramaters));
    }

    public void Interact(InteractionModel model)
    {
        var args = new InteractingEventArgs(model);
        RaiseEvent(ProcessorEvent.Interacting, args);
    }
}
