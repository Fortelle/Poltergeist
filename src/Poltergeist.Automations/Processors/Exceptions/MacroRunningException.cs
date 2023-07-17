using System;

namespace Poltergeist.Automations.Exceptions;

public class MacroRunningException : Exception
{
    public MacroRunningException(string message) : base(message)
    {

    }
}
