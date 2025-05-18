using Microsoft.Win32;

namespace Poltergeist.Automations.Utilities;

public static class RegistryUtil
{
    private static readonly string[] RegPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\",
        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\",
    ];

    public static RegistryKey? FindInstalledApp(string appName)
    {
        foreach (var root in new RegistryKey[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            foreach (var path in RegPaths)
            {
                var subkey = root.OpenSubKey(path + appName);
                if (subkey is not null)
                {
                    return subkey;
                }
            }
        }
        return null;
    }

    public static Dictionary<string, object>? GetValues(RegistryKey subkey)
    {
        var dict = new Dictionary<string, object>();
        foreach (var name in subkey.GetValueNames())
        {
            dict.Add(name, subkey.GetValue(name)!);
        }
        return dict;
    }

}
