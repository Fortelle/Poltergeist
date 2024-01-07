using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.UI;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Windows.UI;

namespace Poltergeist.Automations.Logging;

public class MacroLogger : KernelService
{
    public const string FileLogLevelKey = "logger.tofile";
    public const string FrontLogLevelKey = "logger.toconsole";

    private readonly LoggerOptions Options;

    private readonly Stream? LogFileStream;
    private readonly TextWriter? LogFileWriter;
    private readonly BlockingCollection<string>? WritingQueue;
    //private readonly Task WritingTask;

    private TextInstrument? LogInstrument;

    private bool IsReady = false;

    private readonly Dictionary<LogLevel, Color> LogColors = new()
    {
        [LogLevel.Debug] = (Colors.Gray),
        [LogLevel.Trace] = (Colors.SkyBlue),
        [LogLevel.Information] = (Colors.Black),
        [LogLevel.Warning] = (Colors.Orange),
        [LogLevel.Error] = (Colors.Red),
        [LogLevel.Critical] = (Colors.DarkRed),
        [LogLevel.None] = (Colors.Black),
    };

    public MacroLogger(MacroProcessor processor, IOptions<LoggerOptions> options, PanelService panelService) : base(processor)
    {
        Options = options.Value;

        if(Options.Filename is not null)
        {
            WritingQueue = new(256);

            var fileInfo = new FileInfo(Options.Filename);
            if (fileInfo.Directory is not null && fileInfo.Directory.FullName != fileInfo.Directory.Root.FullName)
            {
                fileInfo.Directory.Create();
            }
            LogFileStream = new FileStream(Options.Filename, FileMode.OpenOrCreate, FileAccess.Write);
            LogFileWriter = new StreamWriter(LogFileStream);

            /* WritingTask = */
            Task.Factory.StartNew(WriteFile, this, TaskCreationOptions.LongRunning);
        }
    }

    internal void Load()
    {
        if (IsReady)
        {
            return;
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

        Log(LogLevel.Debug, nameof(MacroLogger), $"<{nameof(MacroLogger)}> is launched.");

    }

    public void Log(LogLevel logLevel, string sender, string message)
    {
        if (!IsReady)
        {
            return;
        }

        if (!IsEnabled(logLevel))
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

        if (logLevel >= Options.FileLogLevel)
        {
            ToFile(entry);
        }
        if (logLevel >= Options.FrontLogLevel)
        {
            ToFront(entry);
        }
    }

    private bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= Options.FileLogLevel || logLevel >= Options.FrontLogLevel;
    }

    private void ToFile(LogEntry entry)
    {
        if(WritingQueue is null)
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
        var line = new TextLine()
        {
            Text = entry.Message,
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

    public override void Dispose()
    {
        base.Dispose();

        if(WritingQueue is not null)
        {
            WritingQueue.CompleteAdding();

            Task.Run(async () => {
                await Task.Delay(1000);

                LogFileWriter?.Close();
                LogFileStream?.Close();
                WritingQueue?.Dispose();
            });
        }
    }

}
