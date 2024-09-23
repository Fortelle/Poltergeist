﻿using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationStartedHook(int index, DateTime startTime) : MacroHook
{
    public int Index { get => index; set => index = value; }
    public DateTime StartTime { get => startTime; set => startTime = value; }
}