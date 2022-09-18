using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poltergeist.Automations.Exceptions;

public class MacroRunningException : Exception
{
    public MacroRunningException(string message) : base(message)
    {

    }
}
