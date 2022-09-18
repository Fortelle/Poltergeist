using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poltergeist.Automations.Processors.Events;

public class MacroStartedEventArgs : EventArgs
{
    public DateTime StartTime;

    public MacroStartedEventArgs(DateTime startTime)
    {
        StartTime = startTime;
    }
}
