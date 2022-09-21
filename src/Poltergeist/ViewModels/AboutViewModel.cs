using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.ViewModels;

public class AboutViewModel : ObservableRecipient
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public string Authors { get; set; }
    public string License { get; set; }

    public AboutViewModel()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();

        if (assembly != null && assemblyName != null)
        {
            Name = assemblyName.Name;
            Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            Version = assembly.GetName().Version?.ToString();

        }

    }

}
