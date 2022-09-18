using System;
using System.IO;

namespace Poltergeist.Services;

public class PathService
{
    public const string ApplicationName = "Poltergeist";

    public string AppFolder;
    public string DocumentFolder;
    public string LocalDataFolder;
    public string LocalSettingsFile;
    public string SchedulerFile;

    public PathService()
    {
        AppFolder = AppDomain.CurrentDomain.BaseDirectory;

        var mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        DocumentFolder = Path.Combine(mydocPath, ApplicationName);

        LocalDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        LocalSettingsFile = Path.Combine(DocumentFolder, "LocalSettings.json");
        SchedulerFile = Path.Combine(DocumentFolder, "Scheduler.json");
    }
}
