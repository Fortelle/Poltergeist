using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Android.Adb;

public class AdbService : MacroService
{
    public const string KeepAliveKey = "adb.keep_alive";
    public const string IpAddressKey = "adb.ip_address";
    public const string ExePathKey = "adb.exepath";

    public string? WorkingDirectory { get; set; }
    public string? Filename { get; set; }
    public string? Address { get; set; }

    private readonly TerminalService Terminal;

    private bool IsInitialized;
    private bool IsClosed;

    public AdbService(MacroProcessor processor,
        TerminalService terminal
        ) : base(processor)
    {
        Terminal = terminal;
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
            var path = Processor.Options.Get(ExePathKey, "");
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
            Address = Processor.Options.Get(IpAddressKey, "");
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

        Terminal.WorkingDirectory = WorkingDirectory;
        Terminal.PanelHeader = "ADB";
        Terminal.PanelName = "poltergeist-terminal-adb";

        Terminal.Start();
        IsInitialized = true;

        Logger.Debug($"Initialized <{nameof(AdbService)}>.", new { Address, WorkingDirectory, Filename });
    }

    public bool Connect()
    {
        if (!IsInitialized)
        {
            Initialize();
        }

        var keepalive = Processor.Options.Get(KeepAliveKey, false);

        Logger.Debug($"Connecting to adb server {Address}.", new { Address, keepalive });

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
        return Terminal.Execute($"{Filename} {s}");
    }

    public byte[] ExecOut(params string[] args)
    {
        var cmd = new CmdExecutor(WorkingDirectory!)
        {
            AsBinary = true,
        };

        cmd.TryExecute(Filename!, "exec-out", string.Join(' ', args));
        var buff = cmd.OutputData;
        var length = buff!.Length;
        var list = new List<byte>(length);
        for (var i = 0; i < length; i++)
        {
            var b = buff[i];
            if (b == 0x0D && i + 1 < length && buff[i + 1] == 0x0A) // warning: unsafe
            {
                list.Add(0x0A);
                i++;
            }
            else
            {
                list.Add(b);
            }
        }
        return list.ToArray();
    }
}
