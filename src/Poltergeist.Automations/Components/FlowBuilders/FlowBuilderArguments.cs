using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.Automations.Components.FlowBuilders;

public partial class FlowBuilderArguments : ObservableObject
{
    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private string? _subtext;

    [ObservableProperty]
    private double _max;

    [ObservableProperty]
    private double _current;
}
