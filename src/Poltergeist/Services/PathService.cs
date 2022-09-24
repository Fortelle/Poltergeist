using System;
using System.IO;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Services;

public class PathService
{
    public const string ApplicationName = "Poltergeist";

    public string AppFolder;
    public string DocumentFolder;
    public string LocalDataFolder;
    public string LocalSettingsFile;
    public string GlobalMacroOptionsFile;
    public string SchedulerFile;

    public string SharedFolder;
    public string MacroFolder;
    public string GroupFolder;

    public PathService()
    {
        AppFolder = AppDomain.CurrentDomain.BaseDirectory;

        var mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        DocumentFolder = Path.Combine(mydocPath, ApplicationName);

        LocalDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);


        MacroFolder = Path.Combine(DocumentFolder, "Macros");
        SharedFolder = Path.Combine(DocumentFolder, "Shared");
        GroupFolder = Path.Combine(DocumentFolder, "Groups");

        LocalSettingsFile = Path.Combine(DocumentFolder, "LocalSettings.json");
        GlobalMacroOptionsFile = Path.Combine(DocumentFolder, "GlobalOptions.json");
        SchedulerFile = Path.Combine(DocumentFolder, "Scheduler.json");

    }

    public string GetMacroFolder(IMacroBase macro)
    {
        return Path.Combine(MacroFolder, macro.Name);
    }

    public string GetGroupFolder(MacroGroup group)
    {
        return Path.Combine(GroupFolder, group.Name);
    }

}
