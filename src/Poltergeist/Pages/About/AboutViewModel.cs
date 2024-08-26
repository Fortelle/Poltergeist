using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Attributes;

namespace Poltergeist.ViewModels;

public class AboutViewModel : ObservableRecipient
{
    public string? Name { get; }
    public string? Version { get; }
    public string? Description { get; }
    public string? License { get; }
    public Uri? LicenseUrl { get; }
    public string? GitHub { get; }
    public Uri? GitHubUrl { get; }

    public AboutViewModel()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Name = assembly.GetName().Name;
        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        Version = assembly.GetName().Version?.ToString();

        License = assembly.GetCustomAttribute<AssemblyLicenseAttribute>()?.License;

        var licenceUrl = assembly.GetCustomAttribute<AssemblyLicenseUrlAttribute>()?.Url;
        if (licenceUrl is not null)
        {
            LicenseUrl = new Uri(licenceUrl);
        }

        GitHub = assembly.GetCustomAttribute<AssemblyGitHubAttribute>()?.Name;

        var githubUrl = assembly.GetCustomAttribute<AssemblyGitHubUrlAttribute>()?.Url;
        if (githubUrl is not null)
        {
            GitHubUrl = new Uri(githubUrl);
        }
    }

}
