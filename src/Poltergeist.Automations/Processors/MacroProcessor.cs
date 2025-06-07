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

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public ServiceCollection? ServiceCollection { get; private set; }
    public ServiceProvider? ServiceProvider { get; private set; }
    ServiceCollection IConfigurableProcessor.Services => ServiceCollection!;

    /// <summary>
    /// Gets a collection that contains the options for the macro processor.
    /// </summary>
    /// <remarks>
    /// Be aware of modifying this collection during the processor is running, as it may cause unexpected inconsistencies.
    /// The changes to this collection are only effective for the processor and will not affect the macro itself.
    /// </remarks>
    public ParameterValueCollection Options { get; } = new();

    /// <summary>
    /// Gets a collection that contains the environment variables for the macro processor.
    /// </summary>
    public ParameterValueCollection Environments { get; } = new();

    /// <summary>
    /// Gets a collection that contains the statistics for the macro processor.
    /// </summary>
    public ParameterValueCollection Statistics { get; } = new();

    /// <summary>
    /// Gets a collection that contains temporary data for the macro processor.
    /// </summary>
    /// <remarks>
    /// The data stored in this collection are only kept for the lifetime of the processor and will be disposed automatically.
    /// </remarks>
    public ParameterValueCollection SessionStorage { get; } = new();

    /// <summary>
    /// Gets a collection that contains the outcome data produced by the macro processor.
    /// </summary>
    public ParameterValueCollection OutputStorage { get; } = new();

    public LaunchReason Reason { get; private set; }

    public string? Comment { get; set; }

    public Exception? Exception { get; private set; }

    public ProcessorStatus Status { get; private set; } = ProcessorStatus.Idle;

    private IBackMacro Macro { get; }
    IUserMacro IUserProcessor.Macro => (IUserMacro)Macro;
    IUserMacro IPreparableProcessor.Macro => (IUserMacro)Macro;
    IFrontMacro IFrontProcessor.Macro => (IFrontMacro)Macro;

    private LoggerWrapper? Logger;

    public MacroProcessor(MacroBase macro)
    {
        Macro = macro;
        ProcessId = Guid.NewGuid().ToString();

        Macro.Initialize();

        if (!Macro.CheckValidity(out var invalidationMessage))
        {
            Exception = new Exception(invalidationMessage);
            Status = ProcessorStatus.Invalid;
            return;
        }

        if (macro.UserOptions?.Count > 0)
        {
            foreach (var entry in macro.UserOptions)
            {
                Options.TryAdd(entry.Key, entry.DefaultValue);
            }
        }

        if (macro.Statistics?.Count > 0)
        {
            foreach (var entry in macro.Statistics)
            {
                Statistics.TryAdd(entry.Key, entry.DefaultValue);
            }
        }
    }

    public MacroProcessor(MacroBase macro, MacroProcessorArguments arguments) : this(macro)
    {
        Reason = arguments.LaunchReason;

        if (arguments.Options?.Count > 0)
        {
            foreach (var (key, value) in arguments.Options)
            {
                Options.AddOrUpdate(key, value);
            }
        }

        if (arguments.Environments?.Count > 0)
        {
            foreach (var (key, value) in arguments.Environments)
            {
                Environments.AddOrUpdate(key, value);
            }
        }

        if (arguments.SessionStorage?.Count > 0)
        {
            foreach (var (key, value) in arguments.SessionStorage)
            {
                SessionStorage.AddOrUpdate(key, value);
            }
        }

        if (arguments.Statistics is not null && !this.IsIncognitoMode())
        {
            foreach (var (key, value) in arguments.Statistics)
            {
                Statistics.AddOrUpdate(key, value);
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
