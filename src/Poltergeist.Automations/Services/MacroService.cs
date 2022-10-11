using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class MacroService : IExtensionService, IServiceLogger, IDisposable
{
    protected IServiceProcessor Processor { get; }
    protected IServiceLogger Logger { get; }

    private MacroLogger _logger;
    private MacroLogger _Logger => _logger ??= Processor.GetService<MacroLogger>();

    private string SenderName { get; }

    protected MacroService(MacroProcessor processor)
    {
        Processor = processor;
        Logger = this;

        SenderName = GetType().Name;
    }

    void IServiceLogger.Trace(string message)
    {
        _Logger.Log(LogLevel.Trace, SenderName, message);
    }

    void IServiceLogger.Trace(object variable, string name)
    {
        var text = JsonConvert.SerializeObject(variable);
        var message = $"{name}={text}";
        _Logger.Log(LogLevel.Trace, SenderName, message);
    }

    void IServiceLogger.Debug(string message)
    {
        _Logger.Log(LogLevel.Debug, SenderName, message);
    }

    void IServiceLogger.Debug(string message, object[] variables)
    {
        var text = string.Join(", ", variables.Select(ConvertToString));
        message += $" ({text})";
        _Logger.Log(LogLevel.Debug, SenderName, message);
    }

    private static string ConvertToString(object item)
    {
        return item switch
        {
            null => "(null)",
            string s => '"' + s + '"',
            IEnumerable ie => '[' + string.Join(", ", ie.Cast<object>().Select(ConvertToString)),
            _ => item.ToString()
        };
    }

    void IServiceLogger.Info(string message)
    {
        _Logger.Log(LogLevel.Information, SenderName, message);
    }

    void IServiceLogger.Warn(string message)
    {
        _Logger.Log(LogLevel.Warning, SenderName, message);
    }

    void IServiceLogger.Error(string message)
    {
        _Logger.Log(LogLevel.Error, SenderName, message);
    }

    void IServiceLogger.Critical(string message)
    {
        _Logger.Log(LogLevel.Critical, SenderName, message);
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
    public T Macro => Processor.Macro as T;

    protected MacroService(MacroProcessor processor) : base(processor)
    {
    }

}
