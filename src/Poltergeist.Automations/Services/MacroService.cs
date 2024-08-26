using System.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class MacroService : IExtensionService, IServiceLogger, IDisposable
{
    protected IServiceProcessor Processor { get; }
    protected IServiceLogger Logger { get; }

    protected bool IsDisposed;

    private MacroLogger? _loggerProvider;
    private MacroLogger LoggerProvider => _loggerProvider ??= Processor.GetService<MacroLogger>();

    private readonly string SenderName;

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

    IUserProcessor IExtensionService.GetProcessor() => Processor;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        Dispose(true);
        Debug.WriteLine($"Disposed {SenderName}.");
        GC.SuppressFinalize(this);
    }
}

public abstract class MacroService<T>(MacroProcessor processor) : MacroService(processor) where T : MacroBase
{
    public T Macro => (T)Processor.Macro;
}
