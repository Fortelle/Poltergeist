using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Foreground;

public class ForegroundOperator(MacroProcessor processor,
    ForegroundLocatingService locating,
    ForegroundCapturingService capture,
    ForegroundKeyboardService keyboard,
    ForegroundMouseService mouse,
    TimerService timer
    ) : LoopExecuteArguments(processor)
{
    public ForegroundLocatingService Locating => locating;
    public ForegroundCapturingService Capture => capture;
    public ForegroundMouseService Mouse => mouse;
    public ForegroundKeyboardService Keyboard => keyboard;
    public TimerService Timer => timer;
}
