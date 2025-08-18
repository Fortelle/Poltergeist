using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public sealed partial class MacroProcessor : IFrontProcessor, IServiceProcessor, IConfigurableProcessor, IUserProcessor, IPreparableProcessor
{
    public string ProcessorId { get; }

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

    /// <summary>
    /// Gets a collection that contains the processing report.
    /// </summary>
    public ParameterValueCollection Report { get; } = new();

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
        ProcessorId = Guid.NewGuid().ToString();

        Macro.Initialize();

        if (!Macro.CheckValidity(out var invalidationMessage))
        {
            Exception = new Exception(invalidationMessage);
            Status = ProcessorStatus.Invalid;
            return;
        }

        if (macro.OptionDefinitions?.Count > 0)
        {
            foreach (var entry in macro.OptionDefinitions)
            {
                Options.TryAdd(entry.Key, entry.DefaultValue);
            }
        }
    }

    public MacroProcessor(MacroBase macro, MacroProcessorArguments arguments) : this(macro)
    {
        if (Status != ProcessorStatus.Idle)
        {
            return;
        }

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

        if (arguments.LaunchReason != LaunchReason.Unknown)
        {
            Report.Add("launch_reason", arguments.LaunchReason);
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

    /// <summary>
    /// Executes the specified macro using the provided arguments and returns the result.
    /// </summary>
    /// <remarks>This method creates a new instance of <see cref="MacroProcessor"/> to process the macro.</remarks>
    /// <param name="macro">The macro to be processed.</param>
    /// <param name="arguments">Optional arguments for the macro processor.</param>
    /// <returns>A <see cref="ProcessorResult"/> representing the outcome of the macro execution.</returns>
    public static ProcessorResult Execute(MacroBase macro, MacroProcessorArguments? arguments = null)
    {
        using var processor = arguments is null
            ? new MacroProcessor(macro)
            : new MacroProcessor(macro, arguments);
        var result = processor.Execute();
        return result;
    }

    /// <summary>
    /// Executes the specified macro asynchronously using the provided processor arguments and returns the result.
    /// </summary>
    /// <remarks>This method creates a new instance of <see cref="MacroProcessor"/> to process the macro.</remarks>
    /// <param name="macro">The macro to be processed.</param>
    /// <param name="arguments">Optional arguments for the macro processor.</param>
    /// <returns>A <see cref="ProcessorResult"/> representing the outcome of the macro execution.</returns>
    public static async Task<ProcessorResult> ExecuteAsync(MacroBase macro, MacroProcessorArguments? arguments = null)
    {
        using var processor = arguments is null
            ? new MacroProcessor(macro)
            : new MacroProcessor(macro, arguments);
        var result = await processor.ExecuteAsync();
        return result;
    }
}
