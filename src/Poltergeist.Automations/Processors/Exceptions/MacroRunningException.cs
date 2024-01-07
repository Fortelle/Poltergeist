using System;

namespace Poltergeist.Automations.Processors;

public class MacroRunningException : Exception
{
    public MacroRunningException(string message) : base(message)
    {

    }
}
