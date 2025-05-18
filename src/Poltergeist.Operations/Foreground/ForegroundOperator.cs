using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Foreground;

public class ForegroundOperator(MacroProcessor processor,
    ForegroundLocatingService locating,
    ForegroundCapturingService capturing,
    ForegroundKeyboardService keyboard,
    ForegroundMouseService mouse,
    TimerService timer
    ) : IterationArguments(processor)
{
    public ForegroundLocatingService Locating => locating;
    public ForegroundCapturingService Capturing => capturing;
    public ForegroundMouseService Mouse => mouse;
    public ForegroundKeyboardService Keyboard => keyboard;
    public TimerService Timer => timer;
}
