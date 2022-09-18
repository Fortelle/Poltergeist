using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.ViewModels;

public class SchedulerViewModel : ObservableRecipient
{
    public  string _versionDescription;

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public SchedulerViewModel()
    {
        VersionDescription = GetVersionDescription();
    }

    private static string GetVersionDescription()
    {
        //var appName = "AppDisplayName".GetLocalized();
        return "";
        //var version = Package.Current.Id.Version;
        
        //return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
