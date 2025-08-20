using Poltergeist.Helpers.Converters;
using Poltergeist.Modules.Macros;

namespace Poltergeist.UI.Pages.Home;

public class MacroInstanceViewModel(MacroInstance instance)
{
    public MacroInstance Instance => instance;

    public string? Title => instance.Title;

    public string? Description => instance.Description;

    public string? Icon => instance.Icon;

    public double Opacity => CanOpen ? 1 : .5;

    public bool IsFavorite => instance.Properties?.IsFavorite == true;

    public bool CanOpen => instance.IsValid;

    public bool CanEdit => !instance.IsLocked;

    public bool CanDelete => !instance.IsLocked;

    public string? RunCount => instance.Properties?.RunCount > 0 ? instance.Properties.RunCount.ToString() : null;

    public string? LastRunTime => instance.Properties?.LastRunTime is not null ? DateTimeToAgoConverter.Convert(instance.Properties.LastRunTime.Value) : null;
}
