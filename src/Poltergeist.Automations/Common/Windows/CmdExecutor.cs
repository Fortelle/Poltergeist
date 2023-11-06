using System.Diagnostics;

namespace Poltergeist.Common.Windows;


public class CmdExecutor
{
    public bool AsBinary { get; set; }

    public string? Output { get; set; }
    public byte[]? OutputData { get; set; }
    public string? Error { get; set; }

    private readonly string WorkingDirectory;

    public bool HasError => !string.IsNullOrEmpty(Error);

    public CmdExecutor(string workingDirectory = "")
    {
        WorkingDirectory = workingDirectory;
    }

    public int TryExecute(string command, params string[] args)
    {
        var commandParts = command.Split(' ', 2);
        var filename = Path.Combine(WorkingDirectory, commandParts[0]);
        var arguments = commandParts.Length > 1 ? commandParts[1] : "";
        if(args.Length > 0)
        {
            arguments += string.Join(' ', args);
        }

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
        if (AsBinary)
        {
            var ms = new MemoryStream();
            so.BaseStream.CopyTo(ms);
            OutputData = ms.ToArray();
        }
        else
        {
            Output = so.ReadToEnd();
        }
        Error = se.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode;
    }
}
