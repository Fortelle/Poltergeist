using System.Diagnostics;

namespace Poltergeist.Common.Windows;

public class CmdHost : IDisposable
{
    private const string BeginToken = "#process_begin";
    private const string CompletionToken = "#process_received";

    public bool IsExited { get; set; }

    private readonly Process CmdProcess;
    private readonly List<string> OutputBuff = new();
    private readonly List<string> ErrorBuff = new();
    private readonly AutoResetEvent AutoEvent = new(false);
    private string? OutputText;
    private bool HasError;

    public CmdHost(string? workingDirectory = "")
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

        return output ?? "";
    }

    public bool TryExecute(string command, out string? output)
    {
        if (IsExited)
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

        if (IsExited)
        {
            Dispose();
        }

        return isSucess;
    }

    public void Dispose()
    {
        IsExited = true;

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
                IsExited = true;
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
