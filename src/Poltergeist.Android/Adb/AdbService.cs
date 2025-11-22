using System.Drawing;
using System.Text.RegularExpressions;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Android.Adb;

public class AdbService : MacroService
{
    public const string KeepAliveKey = "adb.keep_alive";
    public const string IpAddressKey = "adb.ip_address";
    public const string ExePathKey = "adb.exepath";

    public string? WorkingDirectory { get; set; }
    public string? Filename { get; set; }
    public string? Address { get; set; }

    private readonly TerminalService TerminalService;

    private bool IsInitialized;
    private bool IsClosed;

    public AdbService(MacroProcessor processor, TerminalService terminalService) : base(processor)
    {
        TerminalService = terminalService;
    }

    private void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        Logger.Debug($"Initializing <{nameof(AdbService)}>.");

        if (string.IsNullOrEmpty(WorkingDirectory))
        {
            var path = Processor.Options.GetValueOrDefault(ExePathKey, "");
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"{nameof(ExePathKey)} is not set.");
            }
            if (!File.Exists(path))
            {
                throw new ArgumentException($"File \"{path}\" does not exist.");
            }

            WorkingDirectory = Path.GetDirectoryName(path);
            Filename = Path.GetFileName(path);
        }

        if (string.IsNullOrEmpty(Address))
        {
            Address = Processor.Options.GetValueOrDefault(IpAddressKey, "");
        }

        if (string.IsNullOrEmpty(WorkingDirectory))
        {
            throw new ArgumentException($"{nameof(WorkingDirectory)} is not set.");
        }
        if (!Directory.Exists(WorkingDirectory))
        {
            throw new DirectoryNotFoundException($"Directory \"{WorkingDirectory}\" does not exist.");
        }
        if (string.IsNullOrEmpty(Filename))
        {
            throw new ArgumentException($"{nameof(Filename)} is not set.");
        }
        var exePath = Path.Combine(WorkingDirectory, Filename);
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"File {nameof(exePath)} does not exist.");
        }
        if (string.IsNullOrEmpty(Address))
        {
            throw new ArgumentException($"{nameof(Address)} is not set.");
        }

        TerminalService.WorkingDirectory = WorkingDirectory;
        TerminalService.PanelHeader = "ADB";
        TerminalService.PanelName = "poltergeist-terminal-adb";

        TerminalService.Start();
        IsInitialized = true;

        Logger.Debug($"Initialized <{nameof(AdbService)}>.", new { Address, WorkingDirectory, Filename });
    }

    public bool Connect()
    {
        if (!IsInitialized)
        {
            Initialize();
        }

        Logger.Trace($"Connecting to adb server {Address}.");
        Logger.IncreaseIndent();

        var keepalive = Processor.Options.GetValueOrDefault(KeepAliveKey, false);

        if (!keepalive)
        {
            Processor.GetService<HookService>().Register<ProcessorEndingHook>(_ => Close());
        }

        var output = Execute($"connect {Address}");
        if (output.Contains("unable to connect to"))
        {
            Logger.Error(output);
            return false;
        }

        Logger.Info($"Connected to adb server {Address}.");
        Logger.DecreaseIndent();

        var size = GetScreenSize();
        if (size is null)
        {
            return false;
        }
        Logger.Info($"Device size: {size}");

        var adbVersion = GetAdbVersion();
        Logger.Info($"Adb version: {adbVersion}");

        var androidVersion = GetAndroidVersion();
        Logger.Info($"Android version: {androidVersion}");

        Processor.SessionStorage.AddOrUpdate(LocatingProvider.WorkspaceSizeKey, size.Value);

        Processor.GetService<AdbLocatingService>().SetSize(size.Value);

        return true;
    }

    public void Close()
    {
        if (IsClosed)
        {
            return;
        }

        if (!IsInitialized)
        {
            Initialize();
        }

        Execute($"kill-server");

        Logger.Info($"Closed adb server {Address}.");
        IsClosed = true;
    }

    public string Execute(params string[] args)
    {
        var s = string.Join(' ', args);
        return TerminalService.Execute($"{Filename} {s}");
    }

    public string Shell(params string[] args)
    {
        var s = new List<string>(args);
        s.Insert(0, "shell");
        s.Insert(0, $"-s {Address}");

        return TerminalService.Execute($"{Filename} {string.Join(' ', s)}");
    }

    public byte[] ExecOut(params string[] args)
    {
        var s = new List<string>(args);
        s.Insert(0, "exec-out");
        s.Insert(0, $"-s {Address}");

        Logger.Debug($"Executing command: \"{string.Join(" ", s)}\".");

        var cmd = new CmdExecutor(WorkingDirectory!)
        {
            AsBinary = true,
        };
        cmd.TryExecute(Filename!, [.. s]);
        var buff = cmd.OutputData!;

        Logger.Debug($"Received {buff.Length} bytes of data from command execution.");

        return buff;
    }

    private Size? GetScreenSize()
    {
        var output = Shell("wm size");
        var match = Regex.Match(output, @"Physical size: (\d+)x(\d+)");
        if (!match.Success)
        {
            return null;
        }

        var w = int.Parse(match.Groups[1].Value);
        var h = int.Parse(match.Groups[2].Value);
        return new Size(w, h);
    }

    private Version? GetAdbVersion()
    {
        var output = TerminalService.Execute($"{Filename} version");
        var match = Regex.Match(output, @"Android Debug Bridge version ([\d\.]+)");
        if (!match.Success)
        {
            return null;
        }

        if (!Version.TryParse(match.Groups[1].Value, out var version))
        {
            return null;
        }

        return version;
    }

    private Version? GetAndroidVersion()
    {
        var output = Shell($"getprop ro.build.version.release");
        var match = Regex.Match(output, @"([\d\.]+)");
        if (!match.Success)
        {
            return null;
        }

        if (!Version.TryParse(match.Groups[1].Value, out var version))
        {
            return null;
        }

        return version;
    }
}
