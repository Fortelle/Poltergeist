using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Background;

public class BackgroundOperator : LoopExecutionArguments
{
    public BackgroundLocatingService Locating { get; }
    public BackgroundCapturingService Capture { get; }
    public BackgroundKeyboardService Keyboard { get; }
    public BackgroundMouseService Mouse { get; }
    public TimerService Timer { get; }

    public BackgroundOperator(MacroProcessor processor,
        BackgroundLocatingService locating,
        BackgroundCapturingService capture,
        BackgroundKeyboardService keyboard,
        BackgroundMouseService mouse,
        TimerService timer
        ) : base(processor)
    {
        Locating = locating;
        Capture = capture;
        Keyboard = keyboard;
        Mouse = mouse;
        Timer = timer;
    }
}
