using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poltergeist.Automations.Components.Logging;

public class LoggerWrapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private readonly MacroLogger Logger;

    private readonly string Sender;

    public LoggerWrapper(MacroLogger logger, string sender)
    {
        Logger = logger;
        Sender = sender;
    }

    public void Trace(string message, object? data = null)
    {
        Log(LogLevel.Trace, message, data);
    }

    public void Debug(string message, object? data = null)
    {
        Log(LogLevel.Debug, message, data);
    }

    public void Info(string message, object? data = null)
    {
        Log(LogLevel.Information, message, data);
    }

    public void Warn(string message, object? data = null)
    {
        Log(LogLevel.Warning, message, data);
    }

    public void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    public void Error(Exception exception)
    {
        Log(LogLevel.Error, exception.Message);
    }

    public void Critical(string message)
    {
        Log(LogLevel.Critical, message);
    }

    public void IncreaseIndent()
    {
        Logger.IndentLevel += 1;
    }

    public void DecreaseIndent()
    {
        Logger.IndentLevel -= 1;
    }

    private void Log(LogLevel level, string message, object? data = null)
    {
        if (data is not null)
        {
            message += " (" + ConvertToString(data) + ")";
        }

        Logger.Log(level, Sender, message);
    }

    private static string ConvertToString(object item)
    {
        return item switch
        {
            string s => '"' + s + '"',
            _ => JsonSerializer.Serialize(item, SerializerOptions),
        };
    }
}
