using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class MacroService : IExtensionService, IServiceLogger, IDisposable
{
    protected IServiceProcessor Processor { get; }
    protected IServiceLogger Logger { get; }

    private MacroLogger? _loggerProvider;
    private MacroLogger LoggerProvider => _loggerProvider ??= Processor.GetService<MacroLogger>();

    public DebugService? Debugger => System.Diagnostics.Debugger.IsAttached ? Processor.GetService<DebugService>() : null;

    private string SenderName { get; }

    protected MacroService(MacroProcessor processor)
    {
        Processor = processor;
        Logger = this;

        SenderName = GetType().Name;

        Logger.Debug($"Service <{SenderName}> is activated.");
    }

    void IServiceLogger.Trace(string message)
    {
        LoggerProvider.Log(LogLevel.Trace, SenderName, message);
    }

    void IServiceLogger.Trace(object variable, string? name)
    {
        var text = JsonConvert.SerializeObject(variable);
        var message = $"{name}={text}";
        LoggerProvider.Log(LogLevel.Trace, SenderName, message);
    }

    void IServiceLogger.Debug(string message)
    {
        LoggerProvider.Log(LogLevel.Debug, SenderName, message);
    }

    void IServiceLogger.Debug(string message, object[] variables)
    {
        var text = string.Join(", ", variables.Select(ConvertToString));
        message += $" ({text})";
        LoggerProvider.Log(LogLevel.Debug, SenderName, message);
    }

    private static string ConvertToString(object item)
    {
        return item switch
        {
            null => "(null)",
            string s => '"' + s + '"',
            IEnumerable ie => '[' + string.Join(", ", ie.Cast<object>().Select(ConvertToString)),
            _ => JsonConvert.SerializeObject(item),
        };
    }

    void IServiceLogger.Info(string message)
    {
        LoggerProvider.Log(LogLevel.Information, SenderName, message);
    }

    void IServiceLogger.Warn(string message)
    {
        LoggerProvider.Log(LogLevel.Warning, SenderName, message);
    }

    void IServiceLogger.Error(string message)
    {
        LoggerProvider.Log(LogLevel.Error, SenderName, message);
    }

    void IServiceLogger.Critical(string message)
    {
        LoggerProvider.Log(LogLevel.Critical, SenderName, message);
    }

    IUserProcessor IExtensionService.GetProcessor() => (IUserProcessor)Processor;

    public virtual void Dispose()
    {
        Debug.WriteLine($"Disposed {SenderName}.");
    }

}

public abstract class MacroService<T> : MacroService
    where T : MacroBase
{
    public T Macro => (T)Processor.Macro;

    protected MacroService(MacroProcessor processor) : base(processor)
    {
    }

}
