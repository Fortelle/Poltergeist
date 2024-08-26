namespace Poltergeist.Services;

public class PathService
{
    public const string ApplicationName = "Poltergeist";

    public string AppFolder { get; }
    public string DocumentDataFolder { get; }
    public string LocalDataFolder { get; }


    public string LocalSettingsFile { get; }
    public string GlobalMacroOptionsFile { get; }
    public string GlobalMacroStatisticsFile { get; }
    public string MacroPropertiesFile { get; }
    public string SchedulerFile { get; }

    public string SharedFolder { get; }
    public string MacroFolder { get; }

    public string? SolutionFolder { get; }
    public string? ProjectFolder { get; }


    public PathService()
    {
        AppFolder = AppDomain.CurrentDomain.BaseDirectory;

        var mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        DocumentDataFolder = Path.Combine(mydocPath, ApplicationName);

        LocalDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        MacroFolder = Path.Combine(DocumentDataFolder, "Macros");
        SharedFolder = Path.Combine(DocumentDataFolder, "Shared");
       
        LocalSettingsFile = Path.Combine(DocumentDataFolder, "LocalSettings.json");
        GlobalMacroOptionsFile = Path.Combine(DocumentDataFolder, "GlobalOptions.json");
        GlobalMacroStatisticsFile = Path.Combine(DocumentDataFolder, "GlobalStatistics.json");
        MacroPropertiesFile = Path.Combine(DocumentDataFolder, "Properties.json");
        SchedulerFile = Path.Combine(DocumentDataFolder, "Scheduler.json");

        if (App.IsDevelopment)
        {
            ProjectFolder = AppDomain.CurrentDomain.BaseDirectory[..AppDomain.CurrentDomain.BaseDirectory.IndexOf("\\bin\\")];
            SolutionFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @".."));
        }

    }

}
