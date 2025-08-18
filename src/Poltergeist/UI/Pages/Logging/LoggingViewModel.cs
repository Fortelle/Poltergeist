using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Modules.Logging;

namespace Poltergeist.UI.Pages.Logging;

public partial class LoggingViewModel : ObservableRecipient, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    public ObservableCollection<AppLogEntry> LogSource { get; set; }

    private AppLogEntry? _selectedLogEntry;
    internal AppLogEntry? SelectedLogEntry
    {
        get => _selectedLogEntry;
        set
        {
            SetProperty(ref _selectedLogEntry, value);
            if (value is null)
            {
                EntryData = null;
            }
            else
            {
                EntryData = [
                    new("Message", value.Message),
                    new("Level", value.Level.ToString()),
                    new("Sender", value.Sender),
                    new("Timestamp", value.Timestamp.ToString("o")),
                    ..value.Data?.Select(x => new KeyValuePair<string, string>(x.Key, JsonSerializer.Serialize(x.Value, SerializerOptions))) ?? []
                ];
            }
        }
    }

    [ObservableProperty]
    public partial KeyValuePair<string, string>[]? EntryData { get; set; }

    [ObservableProperty]
    public partial string? TotalTime { get; set; }

    private readonly AppLoggingService LoggingService;

    public bool ShowsSender { get; } = false;

    public LoggingViewModel(AppLoggingService loggingService)
    {
        LoggingService = loggingService;
        LogSource = new(loggingService.LogPool);
        loggingService.Logged += LoggingService_Logged;
#if DEBUG
        ShowsSender = true;
#endif
    }

    private void LoggingService_Logged(AppLogEntry entry)
    {
        PoltergeistApplication.TryEnqueue(() =>
        {
            LogSource.Add(entry);
        });
    }

    public void Dispose()
    {
        LoggingService.Logged -= LoggingService_Logged;
    }
}
