using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.UI.Windows;

public partial class ShellViewModel : ObservableRecipient
{
    public bool IsDevelopment { get; }
    public bool IsAdministrator { get; }
    public bool IsExclusiveMacroMode { get; }

    public PerformanceViewModel? Performance { get; }

    public ShellViewModel()
    {
        IsDevelopment = App.Current.IsDevelopment;
        IsExclusiveMacroMode = App.Current.ExclusiveMacroMode is not null;
        IsAdministrator = App.Current.IsAdministrator;

        if (App.Current.IsDevelopment)
        {
            Performance = new();
        }
    }
}
