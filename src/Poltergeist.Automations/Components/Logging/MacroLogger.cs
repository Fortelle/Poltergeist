using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.UI;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;
using Windows.UI;

namespace Poltergeist.Automations.Components.Logging;

public class MacroLogger : KernelService
{
    public const string ToFileLevelKey = "macrologger_tofile";
    public const string ToDashboardLevelKey = "macrologger_todashboard";
    public const string ToProcessorLevelKey = "macrologger_toprocessor";
    public const string ToDebugLevelKey = "macrologger_todebug";
    public const string ToConsoleLevelKey = "macrologger_toconsole";

    private readonly Dictionary<LogLevel, Color> LogColors = new()
    {
        [LogLevel.Debug] = Colors.Gray,
        [LogLevel.Trace] = Colors.DeepSkyBlue,
        [LogLevel.Information] = Colors.Black,
        [LogLevel.Warning] = Colors.Orange,
        [LogLevel.Error] = Colors.Red,
        [LogLevel.Critical] = Colors.DarkRed,
        [LogLevel.None] = Colors.Black,
    };

    private Stream? LogFileStream;
    private TextWriter? LogFileWriter;
    private BlockingCollection<string>? WritingQueue;
    private Exception? WritingException;
    private TextInstrument? LogInstrument;

    private bool IsReady = false;
    private ConcurrentQueue<LogEntry>? LogPool = new();

    public int IndentLevel { get; set; }
    private readonly bool IsTraceEnabled = false;

    private readonly LogLevel ToFileLevel;
    private readonly LogLevel ToFrontLevel;
    private readonly LogLevel ToProcessorLevel;
    private readonly LogLevel ToDebugLevel;
    private readonly LogLevel ToConsoleLevel;

    public MacroLogger(MacroProcessor processor) : base(processor)
    {
        ToFileLevel = processor.Options.GetValueOrDefault(ToFileLevelKey, LogLevel.None);
        ToFrontLevel = processor.Options.GetValueOrDefault(ToDashboardLevelKey, LogLevel.None);
        ToProcessorLevel = processor.Options.GetValueOrDefault(ToProcessorLevelKey, LogLevel.None);
        ToDebugLevel = processor.Options.GetValueOrDefault(ToDebugLevelKey, LogLevel.None);
        ToConsoleLevel = processor.Options.GetValueOrDefault(ToConsoleLevelKey, LogLevel.None);

#if DEBUG
        IsTraceEnabled = true;
#endif
    }

    private string? GetLogFilename()
    {
        if (Processor.IsIncognitoMode())
        {
            return null;
        }

        if (ToFileLevel == LogLevel.None)
        {
            return null;
        }

        if (!Processor.Environments.TryGetValue<string>("private_folder", out var privateFolder))
        {
            return null;
        }

        return Path.Combine(privateFolder, "Logs", $"{Processor.ProcessorId}.log");
    }

    internal void Load()
    {
        var logFilename = GetLogFilename();
        if (logFilename is not null)
        {
            WritingQueue = new(256);

            try
            {
                var fileInfo = new FileInfo(logFilename);
                if (fileInfo.Directory is not null && fileInfo.Directory.FullName != fileInfo.Directory.Root.FullName)
                {
                    fileInfo.Directory.Create();
                }
                LogFileStream = new FileStream(logFilename, FileMode.OpenOrCreate, FileAccess.Write);
                LogFileWriter = new StreamWriter(LogFileStream);

                Task.Factory.StartNew(WriteFile, this, TaskCreationOptions.LongRunning);
            }
            catch (Exception exception)
            {
                WritingException = exception;

                LogFileWriter?.Dispose();
                LogFileWriter = null;
                LogFileStream?.Dispose();
                LogFileStream = null;
                WritingQueue?.Dispose();
                WritingQueue = null;
            }
        }

        LogInstrument = Processor.GetService<TextInstrument>();
        foreach (var (level, color) in LogColors)
        {
            LogInstrument.Templates.Add(level.ToString(), new()
            {
                Foreground = color,
            });
        }

        Processor.GetService<PanelService>().Create(new("poltergeist-logger", ResourceHelper.Localize("Poltergeist.Automations/Resources/Log_Header"), LogInstrument)
        {
            IsFilled = true,
        });

        IsReady = true;

        if (WritingException is not null)
        {
            Log(LogLevel.Warning, nameof(MacroLogger), WritingException.ToString());
        }

        Log(LogLevel.Debug, nameof(MacroLogger), $"Kernel service '{nameof(MacroLogger)}' is instantiated.");

        while (LogPool!.TryDequeue(out var entry))
        {
            Log(entry);
        }
        LogPool = null;
    }


    public void Log(LogLevel logLevel, string sender, string message)
    {
        if (logLevel == LogLevel.Trace && !IsTraceEnabled)
        {
            return;
        }

        var entry = new LogEntry()
        {
            Sender = sender,
            Level = logLevel,
            Message = message,
            Timestamp = DateTime.Now,
            ElapsedTime = Processor.GetElapsedTime(),
            IndentLevel = IndentLevel,
        };

        if (!IsReady)
        {
            LogPool!.Enqueue(entry);
            return;
        }

        Log(entry);
    }

    private void Log(LogEntry entry)
    {
        if (entry.Level >= ToFileLevel)
        {
            ToFile(entry);
        }
        if (entry.Level >= ToFrontLevel)
        {
            ToFront(entry);
        }
        if (entry.Level >= ToProcessorLevel)
        {
            ToProcessor(entry);
        }
        if (entry.Level >= ToDebugLevel)
        {
            ToDebugOutput(entry);
        }
        if (entry.Level >= ToConsoleLevel)
        {
            ToConsoleOutput(entry);
        }
    }
    
    private void ToFile(LogEntry entry)
    {
        if (WritingQueue is null)
        {
            return;
        }

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(entry.Message))
        {
            sb.Append($"{entry.ElapsedTime}");
            sb.Append('\t');
            sb.Append($"{entry.Timestamp:o}");
            sb.Append('\t');
            sb.Append($"{ToShortLevel(entry.Level)}");
            sb.Append('\t');
            sb.Append($"{entry.Sender}");
            sb.Append('\t');
            sb.Append($"{entry.Message}");
        }

        var line = sb.ToString();
        WritingQueue.Add(line);
    }

    private void ToFront(LogEntry entry)
    {
        var message = entry.Message;
#if DEBUG
        if (message == "---")
        {
            message = new string('=', 32);
        }
        else if (entry.Level == LogLevel.Trace && !string.IsNullOrEmpty(entry.Sender))
        {
            message = $"[{entry.Sender}] {message}";
        }
        if (entry.IndentLevel > 0 && ToFrontLevel <= LogLevel.Trace)
        {
            message = new string(' ', 4 * entry.IndentLevel) + message;
        }
#endif

        var line = new TextLine(message)
        {
            TemplateKey = entry.Level.ToString(),
        };

        LogInstrument?.WriteLine(line);
    }

    private void ToProcessor(LogEntry entry)
    {
        var args = new LogWrittenEventArgs(entry);
        Processor.RaiseEvent(ProcessorEvent.LogWritten, args);
    }

    private void ToDebugOutput(LogEntry entry)
    {
        Debug.WriteLine(entry.Message);
    }

    private void ToConsoleOutput(LogEntry entry)
    {
        Console.WriteLine(entry.Message);
    }

    private static string ToShortLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            _ => level.ToString().ToUpper(),
        };
    }

    private void WriteFile(object? state)
    {
        foreach (var message in WritingQueue!.GetConsumingEnumerable())
        {
            LogFileWriter!.WriteLine(message);
        }
        LogFileWriter!.Flush();
    }

    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            if (WritingQueue is not null)
            {
                WritingQueue.CompleteAdding();

                Task.Run(async () =>
                {
                    await Task.Delay(1000);

                    LogFileWriter?.Close();
                    LogFileStream?.Close();
                    WritingQueue?.Dispose();
                    WritingQueue = null;
                });
            }
        }

        base.Dispose(disposing);
    }

}
