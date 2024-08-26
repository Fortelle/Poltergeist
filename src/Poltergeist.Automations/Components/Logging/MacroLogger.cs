using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.UI;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;
using Windows.UI;

namespace Poltergeist.Automations.Components.Logging;

public class MacroLogger : KernelService
{
    public const string FileLogLevelKey = "logger.tofile";
    public const string FrontLogLevelKey = "logger.toconsole";

    private readonly Dictionary<LogLevel, Color> LogColors = new()
    {
        [LogLevel.Debug] = Colors.Gray,
        [LogLevel.Trace] = Colors.SkyBlue,
        [LogLevel.Information] = Colors.Black,
        [LogLevel.Warning] = Colors.Orange,
        [LogLevel.Error] = Colors.Red,
        [LogLevel.Critical] = Colors.DarkRed,
        [LogLevel.None] = Colors.Black,
    };

    private readonly LoggerOptions Options;

    private Stream? LogFileStream;
    private TextWriter? LogFileWriter;
    private BlockingCollection<string>? WritingQueue;
    private Exception? WritingException;
    private TextInstrument? LogInstrument;

    private bool IsReady = false;
    private ConcurrentQueue<LogEntry>? LogPool = new();

    public MacroLogger(MacroProcessor processor, IOptions<LoggerOptions> options) : base(processor)
    {
        Options = options.Value;
    }

    internal void Load()
    {
        if (Options.Filename is not null)
        {
            WritingQueue = new(256);

            try
            {
                var fileInfo = new FileInfo(Options.Filename);
                if (fileInfo.Directory is not null && fileInfo.Directory.FullName != fileInfo.Directory.Root.FullName)
                {
                    fileInfo.Directory.Create();
                }
                LogFileStream = new FileStream(Options.Filename, FileMode.OpenOrCreate, FileAccess.Write);
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
            Log(LogLevel.Error, nameof(MacroLogger), WritingException.ToString());
        }

        Log(LogLevel.Debug, nameof(MacroLogger), $"<{nameof(MacroLogger)}> is launched.");

        while (LogPool!.TryDequeue(out var entry))
        {
            Log(entry);
        }
        LogPool = null;
    }


    public void Log(LogLevel logLevel, string sender, string message)
    {
        if (logLevel < Options.FileLogLevel && logLevel < Options.FrontLogLevel)
        {
            return;
        }

        var entry = new LogEntry()
        {
            Sender = sender,
            Level = logLevel,
            Message = message,
            Timestamp = DateTime.Now,
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
        if (entry.Level >= Options.FileLogLevel)
        {
            ToFile(entry);
        }
        if (entry.Level >= Options.FrontLogLevel)
        {
            ToFront(entry);
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
        var line = new TextLine(entry.Message)
        {
            TemplateKey = entry.Level.ToString(),
        };

        LogInstrument?.WriteLine(line);
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
                });
            }
        }

        base.Dispose(disposing);
    }

}
