using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Poltergeist.Automations.Components.Logging;

public class LoggerWrapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new IntPtrConverter(),
            new JsonStringEnumConverter(),
        },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = {
                static jsonTypeInfo =>
                {
                    foreach (var property in jsonTypeInfo.Properties)
                    {
                        var browsableAttribute = property.AttributeProvider?.GetCustomAttributes(typeof(BrowsableAttribute), true).OfType<BrowsableAttribute>().FirstOrDefault();
                        if (browsableAttribute != null && !browsableAttribute.Browsable)
                        {
                            property.ShouldSerialize = static (_, _) => false;
                        }
                    }
                }
            }
        },
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

    public void Trace(object variable, [CallerArgumentExpression(nameof(variable))] string? variableName = null)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            return;
        }

        Log(LogLevel.Trace, $"{variableName} = {variable}");
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
        Log(LogLevel.Error, exception);
    }

    public void Critical(string message)
    {
        Log(LogLevel.Critical, message);
    }

    public void Critical(Exception exception)
    {
        Log(LogLevel.Critical, exception);
    }

    public void IncreaseIndent()
    {
        Logger.IndentLevel += 1;
    }

    public void DecreaseIndent()
    {
        Logger.IndentLevel = Math.Max(Logger.IndentLevel - 1, 0);
    }

    public void IncreaseIndent(string message)
    {
        Trace(message);
        IncreaseIndent();
    }

    public void DecreaseIndent(string message)
    {
        Trace(message);
        DecreaseIndent();
    }

    public void ResetIndent()
    {
        Logger.IndentLevel = 0;
    }

    public void Log(LogLevel level, string message, object? data = null)
    {
        if (data is string[] lines)
        {
            Logger.Log(level, Sender, message);
            IncreaseIndent();
            foreach (var line in lines)
            {
                Logger.Log(level, string.Empty, line);
            }
            DecreaseIndent();
        }
        else if (data is IEnumerable ie)
        {
            Logger.Log(level, Sender, message);
            IncreaseIndent();
            foreach (var item in ie)
            {
                Logger.Log(level, string.Empty, ConvertToString(item));
            }
            DecreaseIndent();
        }
        else if (data is not null)
        {
            Logger.Log(level, Sender, message + " (" + ConvertToString(data) + ")");
        }
        else
        {
            Logger.Log(level, Sender, message);
        }
    }

    public void Log(LogLevel level, Exception exception)
    {
        Log(level, exception.Message);
        if (exception.InnerException is not null)
        {
            IncreaseIndent();
            Log(level, exception.InnerException);
            DecreaseIndent();
        }
    }

    private static string ConvertToString(object item)
    {
        return item switch
        {
            string s => '"' + s + '"',
            _ => JsonSerializer.Serialize(item, SerializerOptions),
        };
    }

    private class IntPtrConverter : JsonConverter<IntPtr>
    {
        public override nint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, nint value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
