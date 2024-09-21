using System.Diagnostics;

namespace Poltergeist.Modules.App;

public class PathProvider
{
    public string AppFolder { get; }
    public string DocumentDataFolder { get; }
    public string LocalDataFolder { get; }

    public string GlobalMacroOptionsFile { get; }
    public string GlobalMacroStatisticsFile { get; }
    public string MacroPropertiesFile { get; }
    public string SchedulerFile { get; }

    public string SharedFolder { get; }
    public string MacroFolder { get; }
    public string LogFolder { get; }

    public string? SolutionFolder { get; }
    public string? ProjectFolder { get; }


    public PathProvider()
    {
        AppFolder = AppDomain.CurrentDomain.BaseDirectory;

        var mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        DocumentDataFolder = Path.Combine(mydocPath, PoltergeistApplication.ApplicationName);

        LocalDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        MacroFolder = Path.Combine(DocumentDataFolder, "Macros");
        SharedFolder = Path.Combine(DocumentDataFolder, "Shared");
        LogFolder = Path.Combine(DocumentDataFolder, "Logs");

        GlobalMacroOptionsFile = Path.Combine(DocumentDataFolder, "GlobalOptions.json");
        GlobalMacroStatisticsFile = Path.Combine(DocumentDataFolder, "GlobalStatistics.json");
        MacroPropertiesFile = Path.Combine(DocumentDataFolder, "Properties.json");
        SchedulerFile = Path.Combine(DocumentDataFolder, "Scheduler.json");

        if (Debugger.IsAttached)
        {
            ProjectFolder = AppDomain.CurrentDomain.BaseDirectory[..AppDomain.CurrentDomain.BaseDirectory.IndexOf("\\bin\\")];
            SolutionFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @".."));
        }

    }

}
