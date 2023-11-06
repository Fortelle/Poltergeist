using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Repetitions;

[ObservableObject]
public partial class LoopExecutionArguments : ArgumentService
{
    public int Index { get; internal set; }
    public DateTime StartTime { get; internal set; }
    public IterationStatus Result { get; set; }

    [ObservableProperty]
    public int _progressMax;

    [ObservableProperty]
    private int _progressValue;

    public LoopExecutionArguments(MacroProcessor processor) : base(processor)
    {
    }

}
