using System;
using System.Collections.Generic;
using System.IO;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Windows;

namespace Poltergeist.Operations.Android;

public class AdbService : MacroService
{
    public const string AutoCloseKey = "adb.auto_close";
    public const string IpAddressKey = "adb.ip_address";
    public const string ExePathKey = "adb.exepath";

    public string WorkingDirectory { get; set; }
    public string Filename { get; set; }
    public string Address { get; set; }

    private TerminalService Terminal { get; init; }

    private bool IsInitialized { get; set; }
    private bool IsClosed { get; set; }

    public AdbService(MacroProcessor processor,
        TerminalService terminal
        ) : base(processor)
    {
        Terminal = terminal;
    }

    private void Initialize()
    {
        if (IsInitialized) return;

        Logger.Debug($"Initializing <{nameof(AdbService)}>.");

        if (string.IsNullOrEmpty(WorkingDirectory))
        {
            var path = Processor.GetOption(ExePathKey, "");
            if (!string.IsNullOrEmpty(path))
            {
                WorkingDirectory = Path.GetDirectoryName(path);
                Filename = Path.GetFileName(path);
            }
        }

        if (string.IsNullOrEmpty(Address))
        {
            Address = Processor.GetOption(IpAddressKey, "");
        }

        if (string.IsNullOrEmpty(WorkingDirectory))
        {
            throw new ArgumentException($"{nameof(WorkingDirectory)} is not set.");
        }
        if (!Directory.Exists(WorkingDirectory))
        {
            throw new DirectoryNotFoundException($"{WorkingDirectory} cannot be found.");
        }
        if (string.IsNullOrEmpty(Filename))
        {
            throw new ArgumentException($"{nameof(Filename)} is not set.");
        }
        var exePath = Path.Combine(WorkingDirectory, Filename);
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"File {nameof(exePath)} is not set.");
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
        if (!IsInitialized) Initialize();

        var autoclose = Processor.GetOption(AutoCloseKey, false);

        Logger.Debug($"Connecting to {Address}.", new { Address, autoclose });

        if (autoclose)
        {
            Processor.GetService<HookService>().Register("process_exiting", _ => Close());
        }

        var output = Execute($"connect {Address}");
        if(output.Contains("unable to connect to"))
        {
            Logger.Error(output);
            return false;
        }
        return true;
    }

    public void Close()
    {
        if (IsClosed) return;

        if (!IsInitialized) Initialize();

        Execute($"kill-server");
        IsClosed = true;
    }

    public string Execute(params string[] args)
    {
        var s = string.Join(' ', args);
        return Terminal.Execute($"{Filename} {s}");
    }

    public byte[] ExecOut(params string[] args)
    {
        var cmd = new CmdExecutor(WorkingDirectory)
        {
            AsBinary = true,
        };

        cmd.TryExecute(Filename, "exec-out", string.Join(' ', args));
        var buff = cmd.OutputData;
        var length = buff.Length;
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
