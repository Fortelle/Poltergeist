using System.Diagnostics;

namespace Poltergeist.Automations.Utilities;

public class CmdHost : IDisposable
{
    private const string BeginToken = "#process_begin";
    private const string CompletionToken = "#process_received";

    private readonly Process CmdProcess;
    private readonly List<string> OutputBuff = new();
    private readonly List<string> ErrorBuff = new();
    private readonly AutoResetEvent AutoEvent = new(false);
    private string? OutputText;
    private bool HasError;

    private bool IsExited;
    protected bool IsDisposed;

    public CmdHost() : this("")
    {
    }

    public CmdHost(string workingDirectory)
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
        ObjectDisposedException.ThrowIf(IsDisposed, this);

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
        ObjectDisposedException.ThrowIf(IsDisposed, this);

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
        if (e.Data is not null)
        {
            ErrorBuff.Add(e.Data);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            IsExited = true;

            CmdProcess.OutputDataReceived -= OutputDataReceived;
            CmdProcess.ErrorDataReceived -= ErrorDataReceived;
            CmdProcess.Close();

            AutoEvent.Close();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
