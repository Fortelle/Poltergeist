using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Poltergeist.Common.Windows;

public class CmdHost : IDisposable
{
    private const string BeginToken = "#process_begin";
    private const string CompletionToken = "#process_received";

    public bool HasExited { get; set; }

    private Process CmdProcess;
    private List<string> OutputBuff = new();
    private List<string> ErrorBuff = new();
    private string OutputText;
    private bool HasError;
    private AutoResetEvent AutoEvent = new(false);

    public CmdHost(string workingDirectory = "")
    {
        CmdProcess = new Process()
        {
            StartInfo =
            {
                FileName = "cmd",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory,
            },
        };
        CmdProcess.OutputDataReceived += OutputDataReceived;
        CmdProcess.ErrorDataReceived += ErrorDataReceived;
    }

    public void Start()
    {
        CmdProcess.Start();
        CmdProcess.BeginOutputReadLine();
        CmdProcess.BeginErrorReadLine();
        CmdProcess.StandardInput.WriteLine($"echo {BeginToken}");
    }

    public string Execute(string command)
    {
        TryExecute(command, out var output);

        return output;
    }

    public bool TryExecute(string command, out string output)
    {
        if (HasExited)
        {
            throw new ObjectDisposedException(nameof(CmdProcess), "The process has exited.");
        }

        CmdProcess.StandardInput.WriteLine(command);
        CmdProcess.StandardInput.WriteLine($"echo {CompletionToken}");
        CmdProcess.StandardInput.Flush();

        AutoEvent.WaitOne();

        var isSucess = !HasError;
        output = OutputText;
        HasError = false;

        if (HasExited)
        {
            Dispose();
        }

        return isSucess;
    }

    public void Dispose()
    {
        HasExited = true;

        CmdProcess.OutputDataReceived -= OutputDataReceived;
        CmdProcess.ErrorDataReceived -= ErrorDataReceived;
        CmdProcess.Close();

        AutoEvent.Close();
    }

    private void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        switch (e.Data)
        {
            case BeginToken:
                OutputBuff.Clear();
                break;
            case CompletionToken:
                BuildText();
                AutoEvent.Set();
                break;
            case null: // exit
                HasExited = true;
                AutoEvent.Set();
                break;
            default:
                OutputBuff.Add(e.Data);
                break;
        }
    }

    private void BuildText()
    {
        HasError = ErrorBuff.Count > 0;
        OutputText = HasError
            ? string.Join('\n', ErrorBuff)
            : string.Join('\n', OutputBuff.SkipWhile(x => x.Length == 0).Take(1..^2));
        ErrorBuff.Clear();
        OutputBuff.Clear();
    }

    private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            ErrorBuff.Add(e.Data);
        }
    }

}

public class CmdExecutor
{
    public string Output { get; set; }
    public string Error { get; set; }

    private readonly string WorkingDirectory;

    public bool HasError => !string.IsNullOrEmpty(Error);

    public CmdExecutor(string workingDirectory = "")
    {
        WorkingDirectory = workingDirectory;
    }

    public int TryExecute(string command)
    {
        var commandParts = command.Split(' ', 2);
        var filename = Path.Combine(WorkingDirectory, commandParts[0]);
        var arguments = commandParts.Length > 1 ? commandParts[1] : "";
        using var process = new Process()
        {
            StartInfo =
            {
                FileName = filename,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        process.Start();
        using var se = process.StandardError;
        using var so = process.StandardOutput;
        Error = se.ReadToEnd();
        Output = so.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode;
    }

}
