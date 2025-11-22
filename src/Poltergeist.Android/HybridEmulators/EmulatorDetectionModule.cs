using System.Text.RegularExpressions;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Android.HybridEmulators;

public class EmulatorDetectionModule : MacroModule
{
    private const string ConfigKey = "autodetect_emulator";

    private static readonly Dictionary<string, Func<IPreparableProcessor, bool>> EmulatorDetections = new()
    {
        { "Android Studio", DetectAndroidStudio },
        { "BlueStacks", DetectBlueStacks },
        { "LDPlayer9", DetectLDPlayer },
        { "Mumu", DetectMumu },
        { "Nox", DetectNox },
    };

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.OptionDefinitions.Add(new ChoiceOption<string>(ConfigKey,
        [
            "Custom",
            "Any",
            .. EmulatorDetections.Keys,
        ])
        {
            DisplayLabel = "Emulator",
            Category = "ADB",
            IsGlobal = true,
        });
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        var emulator = processor.Options.GetValueOrDefault<string>(ConfigKey);
        if (string.IsNullOrEmpty(emulator) || emulator == "Custom")
        {
        }
        else if (emulator == "Any")
        {
            throw new NotImplementedException();
        }
        else if (!EmulatorDetections.ContainsKey(emulator))
        {
            throw new NotImplementedException();
        }
        else if (EmulatorDetections.TryGetValue(emulator, out var func))
        {
            if (func(processor))
            {
                return;
            }
            else
            {
                throw new Exception($"Emulator '{emulator}' is not installed correctly.");
            }
        }
    }

    private static bool DetectAndroidStudio(IPreparableProcessor processor)
    {
        var adbpath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Android\sdk\platform-tools\adb.exe");
        if (!File.Exists(adbpath))
        {
            return false;
        }

        var subkey = RegistryUtil.FindInstalledApp("Android Studio");
        if (subkey is null)
        {
            return false;
        }
        if (subkey.GetValue(@"UninstallString") is not string uninstallString)
        {
            return false;
        }
        var is32 = subkey.Name.Contains("Wow6432Node");

        var emulatorpath = Path.Combine(Path.GetDirectoryName(uninstallString)!, "bin", is32 ? "studio.exe" : "studio64.exe");
        if (!File.Exists(emulatorpath))
        {
            return false;
        }

        processor.Options.AddOrUpdate("emulator.exepath", emulatorpath);
        processor.Options.AddOrUpdate(AdbService.IpAddressKey, @"127.0.0.1:5037");
        processor.Options.AddOrUpdate(AdbService.ExePathKey, adbpath);

        processor.SessionStorage.AddOrUpdate("window_region_config", new RegionConfig()
        {
            ClassName = "Qt653QWindowIcon",
            ChildClassName = "Qt672QWindowIcon"
        });

        return true;
    }

    private static bool DetectBlueStacks(IPreparableProcessor processor)
    {
        var subkey = RegistryUtil.FindInstalledApp("BlueStacks_nxt");
        if (subkey is null)
        {
            return false;
        }
        if (subkey.GetValue(@"UninstallString") is not string uninstallString)
        {
            return false;
        }

        var installPath = Path.GetDirectoryName(CleanPath(uninstallString))!;

        var emulatorpath = Path.Combine(installPath, @"HD-Player.exe");
        if (!File.Exists(emulatorpath))
        {
            return false;
        }

        var adbpath = Path.Combine(installPath, @"HD-Adb.exe");
        if (!File.Exists(adbpath))
        {
            return false;
        }

        processor.Options.AddOrUpdate("emulator.exepath", emulatorpath);
        processor.Options.AddOrUpdate(AdbService.IpAddressKey, @"127.0.0.1:5555");
        processor.Options.AddOrUpdate(AdbService.ExePathKey, adbpath);

        processor.SessionStorage.AddOrUpdate("window_region_config", new RegionConfig()
        {
            WindowName = "BlueStacks App Player",
            ClassName = "Qt672QWindowIcon",
            ChildClassName = "Qt672QWindowIcon",
        });

        return true;
    }

    private static bool DetectLDPlayer(IPreparableProcessor processor)
    {
        var subkey = RegistryUtil.FindInstalledApp("LDPlayer9");
        if (subkey is null)
        {
            return false;
        }
        if (subkey.GetValue(@"UninstallString") is not string uninstallString)
        {
            return false;
        }

        var installPath = Path.GetDirectoryName(CleanPath(uninstallString))!;

        var emulatorpath = Path.Combine(installPath, @"LDPlayer.exe");
        if (!File.Exists(emulatorpath))
        {
            return false;
        }

        var adbpath = Path.Combine(installPath, @"adb.exe");
        if (!File.Exists(adbpath))
        {
            return false;
        }

        processor.Options.AddOrUpdate("emulator.exepath", emulatorpath);
        processor.Options.AddOrUpdate(AdbService.IpAddressKey, @"127.0.0.1:16384");
        processor.Options.AddOrUpdate(AdbService.ExePathKey, adbpath);

        processor.SessionStorage.AddOrUpdate("window_region_config", new RegionConfig()
        {
            ClassName = "LDPlayerMainFrame",
            ChildClassName = "subWin"
        });

        return true;
    }

    private static bool DetectMumu(IPreparableProcessor processor)
    {
        var subkey = RegistryUtil.FindInstalledApp("MuMuPlayer");
        if (subkey is null)
        {
            return false;
        }
        if (subkey.GetValue(@"UninstallString") is not string uninstallString)
        {
            return false;
        }

        var installPath = Path.GetDirectoryName(CleanPath(uninstallString))!;

        var emulatorpath = Path.Combine(installPath, @"nx_main\MuMuNxMain.exe");
        if (!File.Exists(emulatorpath))
        {
            return false;
        }

        var adbpath = Path.Combine(installPath, @"shell\adb.exe");
        if (!File.Exists(adbpath))
        {
            return false;
        }

        processor.Options.AddOrUpdate("emulator.exepath", emulatorpath);
        processor.Options.AddOrUpdate(AdbService.IpAddressKey, @"127.0.0.1:16384");
        processor.Options.AddOrUpdate(AdbService.ExePathKey, adbpath);

        processor.SessionStorage.AddOrUpdate("window_region_config", new RegionConfig()
        {
            ClassName = "Qt5156QWindowIcon",
            ChildClassName = "Qt5156QWindowIcon",
        });

        return true;
    }

    private static bool DetectNox(IPreparableProcessor processor)
    {
        var subkey = RegistryUtil.FindInstalledApp("Nox");
        if (subkey is null)
        {
            return false;
        }
        if (subkey.GetValue(@"UninstallString") is not string uninstallString)
        {
            return false;
        }

        var installPath = Path.GetDirectoryName(CleanPath(uninstallString))!;

        var emulatorpath = Path.Combine(installPath, @"Nox.exe");
        if (!File.Exists(emulatorpath))
        {
            return false;
        }

        var adbpath = Path.Combine(installPath, @"adb.exe");
        if (!File.Exists(adbpath))
        {
            return false;
        }

        processor.Options.AddOrUpdate("emulator.exepath", emulatorpath);
        processor.Options.AddOrUpdate(AdbService.IpAddressKey, @"127.0.0.1:62001");
        processor.Options.AddOrUpdate(AdbService.ExePathKey, adbpath);

        processor.SessionStorage.AddOrUpdate("window_region_config", new RegionConfig()
        {
            ClassName = "Qt5QWindowIcon",
            ChildClassName = "subWin"
        });

        return true;
    }

    private static string CleanPath(string path)
    {
        if (path.StartsWith('"'))
        {
            return Regex.Match(path, @"""(.+?)""").Groups[1].Value;
        }
        return path;
    }

}
