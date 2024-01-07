using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

[ObservableObject]
public partial class LoopExecuteArguments : ArgumentService
{
    public int Index { get; internal set; }
    public DateTime StartTime { get; internal set; }
    public IterationStatus Result { get; set; }

    [ObservableProperty]
    private int _progressMax;

    [ObservableProperty]
    private int _progressValue;

    public LoopExecuteArguments(MacroProcessor processor) : base(processor)
    {
    }
}
