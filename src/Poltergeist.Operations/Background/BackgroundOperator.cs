using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Background;

public class BackgroundOperator(
    MacroProcessor processor,
    BackgroundLocatingService locating,
    BackgroundCapturingService capturing,
    BackgroundKeyboardService keyboard,
    BackgroundMouseService mouse,
    TimerService timer
    ) : IterationArguments(processor)
{
    public BackgroundLocatingService Locating => locating;
    public BackgroundCapturingService Capturing => capturing;
    public BackgroundKeyboardService Keyboard => keyboard;
    public BackgroundMouseService Mouse => mouse;
    public TimerService Timer => timer;
}
