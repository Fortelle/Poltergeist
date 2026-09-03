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
    public string? ExePath { get; set; }
    public string? Address { get; set; }
    public bool IsConnected { get; set; }

    private readonly TerminalService TerminalService;

    private bool IsInitialized;

    private string WorkingDirectory => Path.GetDirectoryName(ExePath)!;
    private string Filename => Path.GetFileName(ExePath)!;

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

        if (string.IsNullOrEmpty(ExePath))
        {
            throw new ArgumentException($"{nameof(AdbService)}.{nameof(ExePath)} is not set.");
        }

        if (!File.Exists(ExePath))
        {
            throw new FileNotFoundException($"File {nameof(ExePath)} does not exist.");
        }

        if (string.IsNullOrEmpty(Address))
        {
            throw new ArgumentException($"{nameof(AdbService)}.{nameof(Address)} is not set.");
        }

        TerminalService.WorkingDirectory = WorkingDirectory;
        TerminalService.PanelHeader = "ADB";
        TerminalService.PanelName = "poltergeist-terminal-adb";

        TerminalService.Start();
        IsInitialized = true;

        Logger.Debug($"Initialized <{nameof(AdbService)}>.", new { Address, ExePath });
    }

    public bool Connect()
    {
        if (!IsInitialized)
        {
            Initialize();
        }

        Logger.Trace($"Connecting to adb server {Address}.");
        Logger.IncreaseIndent();

        var output = Execute($"connect {Address}");
        if (output.Contains("unable to connect to"))
        {
            Logger.Error(output);
            return false;
        }

        Logger.Info($"Connected to adb server {Address}.");
        Logger.DecreaseIndent();

        var size = GetScreenSize();
        Logger.Info($"Device size: {size}");

        var adbVersion = GetAdbVersion();
        Logger.Info($"Adb version: {adbVersion}");

        var androidVersion = GetAndroidVersion();
        Logger.Info($"Android version: {androidVersion}");

        Processor.SessionStorage.AddOrUpdate(LocatingProvider.WorkspaceSizeKey, size);

        Processor.GetService<AdbLocatingService>().SetSize(size);

        Processor.GetService<HookService>().Raise(new AdbConnectedHook()
        {
            Address = Address!,
            ScreenSize = size,
            AdbVersion = adbVersion,
            AandroidVersion = androidVersion,
        });

        IsConnected = true;

        return true;
    }

    public void Close()
    {
        if (!IsConnected)
        {
            return;
        }

        if (!IsInitialized)
        {
            return;
        }

        Execute($"kill-server");

        Processor.GetService<HookService>().Raise(new AdbClosedHook()
        {
            Address = Address!,
        });

        Logger.Info($"Closed adb server {Address}.");
        IsConnected = false;
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

    private Size GetScreenSize()
    {
        var output = Shell("wm size");
        var match = Regex.Match(output, @"Physical size: (\d+)x(\d+)");
        if (!match.Success)
        {
            throw new Exception("Failed to get the screen size.");
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
