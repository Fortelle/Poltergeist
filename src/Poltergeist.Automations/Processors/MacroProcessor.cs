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
    public string? ShellKey { get; }

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

    public Exception? Exception { get; set; }

    private IBackMacro Macro { get; }
    IUserMacro IUserProcessor.Macro => (IUserMacro)Macro;
    IUserMacro IPreparableProcessor.Macro => (IUserMacro)Macro;
    IFrontMacro IFrontProcessor.Macro => (IFrontMacro)Macro;

    private readonly SynchronizationContext? OriginalContext = SynchronizationContext.Current;

    public MacroProcessor(IFrontBackMacro macro, LaunchReason reason)
    {
        Macro = (IBackMacro)macro;
        Reason = reason;
        ProcessId = Guid.NewGuid().ToString();

        Macro.Initialize();

        var invalidationMessage = Macro.CheckValidity();
        if (invalidationMessage is not null)
        {
            Exception = new Exception(invalidationMessage);
            return;
        }
    }

    public MacroProcessor(IFrontBackMacro macro, LaunchReason reason, string shellKey) : this(macro, reason)
    {
        ShellKey = shellKey;
    }

    public T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T))!;
    }

    public object? GetService(Type type)
    {
        return ServiceProvider!.GetService(type);
    }

    private void Log(LogLevel level, string message)
    {
        GetService<MacroLogger>().Log(level, nameof(MacroProcessor), message);
    }

    private void Log(Exception exception, LogLevel level = LogLevel.Error)
    {
        Log(level, exception.Message);

        if (exception.InnerException is not null)
        {
            Log(exception.InnerException, level);
        }
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
