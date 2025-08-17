using System.Collections;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Modules.Logging;

public class AppLogWrapper
{
    private readonly AppLoggingService LogService;
    private readonly string SenderName;

    public AppLogWrapper(object sender)
    {
        LogService = PoltergeistApplication.GetService<AppLoggingService>();
        SenderName = sender.GetType().Name;
    }

    public void Trace(string message, object? data = null)
    {
        Log(AppLogLevel.Trace, message, data);
    }

    public void Debug(string message, object? data = null)
    {
        Log(AppLogLevel.Debug, message, data);
    }

    public void Info(string message, object? data = null)
    {
        Log(AppLogLevel.Information, message, data);
    }

    public void Warn(string message, object? data = null)
    {
        Log(AppLogLevel.Warning, message, data);
    }

    public void Error(string message, object? data = null)
    {
        Log(AppLogLevel.Error, message, data);
    }

    public void Error(Exception exception)
    {
        Log(AppLogLevel.Error, exception.Message);
    }

    public void Critical(string message, object? data = null)
    {
        Log(AppLogLevel.Critical, message, data);
    }

    private void Log(AppLogLevel level, string message, object? data = null)
    {
        LogService.Log(new()
        {
            Sender = SenderName,
            Level = level,
            Message = message,
            Timestamp = DateTime.Now,
            Data = data is not null ? ConvertToDictionary(data) : null,
        });
    }

    public static object? ConvertToObject(object? item)
    {
        return item switch
        {
            null => null,
            string s => s,
            IDictionary id => id.Cast<DictionaryEntry>().ToDictionary(x => x.Key, x => ConvertToObject(x)),
            IEnumerable ie => ie.Cast<object>().Select(x => ConvertToObject(x)),
            _ when StringificationUtil.IsToStringOverridden(item.GetType()) => $"{item}",
            _ => ConvertToDictionary(item),
        }; ;
    }

    private static Dictionary<string, object> ConvertToDictionary(object data)
    {
        var dict = new Dictionary<string, object>();
        if (data is IDictionary id)
        {
            foreach (DictionaryEntry entry in id)
            {
                dict.Add($"{entry.Key}", ConvertToObject(entry.Value) ?? "");
            }
        }
        else
        {
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(data))
            {
                if (propertyDescriptor.Attributes.OfType<JsonIgnoreAttribute>().Any())
                {
                    continue;
                }
                var obj = propertyDescriptor.GetValue(data);
                dict.Add(propertyDescriptor.Name, ConvertToObject(obj) ?? "");
            }
        }
        return dict;
    }

}