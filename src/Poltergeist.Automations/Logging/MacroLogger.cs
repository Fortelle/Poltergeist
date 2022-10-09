using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Logging;

public class MacroLogger : MacroService
{
    private LoggerOptions Options;

    private readonly BlockingCollection<string> writingQueue = new(256);
    private readonly Task WritingTask;
    private Stream LogFileStream;
    private TextWriter LogFileWriter;

    private TextPanelModel LogPanel;

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

    public MacroLogger(MacroProcessor processor, IOptions<LoggerOptions> options) : base(processor)
    {
        Options = options.Value;

        OpenFile();
        WritingTask = Task.Factory.StartNew(WriteFile, this, TaskCreationOptions.LongRunning);

    }

    public void UpdateUI()
    {
        var panelService = Processor.GetService<PanelService>();
        LogPanel = panelService.Create<TextPanelModel>(panel =>
        {
            panel.Key = "poltergeist-logger";
            panel.Header = "Log";

            foreach (var (level, color) in LogColors)
            {
                panel.Colors.Add(level.ToString(), color);
            }
        });
    }

    public void Log(LogLevel logLevel, string sender, string message)
    {
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
        writingQueue.Add(line);
    }

    private void ToFront(LogEntry entry)
    {
        if (LogPanel == null) return;

        var line = new TextLine()
        {
            Text = entry.Message,
            Category = entry.Level.ToString(),
        };

        LogPanel.WriteLine(line);
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

    private void OpenFile()
    {
        var fileInfo = new FileInfo(Options.Filename);
        fileInfo.Directory.Create();
        LogFileStream = new FileStream(Options.Filename, FileMode.OpenOrCreate, FileAccess.Write);
        LogFileWriter = new StreamWriter(LogFileStream);
    }

    private void WriteFile(object state)
    {
        foreach (var message in writingQueue.GetConsumingEnumerable())
        {
            LogFileWriter.WriteLine(message);
        }
        LogFileWriter.Flush();
    }

    private void CloseFile()
    {
        LogFileWriter.Close();
        LogFileStream.Close();
        writingQueue.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();

        writingQueue.CompleteAdding();
        Task.Run(async () => {
            await Task.Delay(1000);
            CloseFile();
        });
    }

}
