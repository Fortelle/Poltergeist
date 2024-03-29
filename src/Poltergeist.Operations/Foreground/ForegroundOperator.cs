﻿using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Foreground;

public class ForegroundOperator : LoopExecuteArguments
{
    public ForegroundLocatingService Locating { get; }
    public ForegroundCapturingService Capture { get; }
    public ForegroundMouseService Mouse { get; }
    public ForegroundKeyboardService Keyboard { get; }
    public TimerService Timer { get; }

    public ForegroundOperator(MacroProcessor processor,
        ForegroundLocatingService locating,
        ForegroundCapturingService capture,
        ForegroundKeyboardService keyboard,
        ForegroundMouseService mouse,
        TimerService timer
        ) : base(processor)
    {
        Locating = locating;
        Capture = capture;
        Mouse = mouse;
        Keyboard = keyboard;
        Timer = timer;
    }
}
