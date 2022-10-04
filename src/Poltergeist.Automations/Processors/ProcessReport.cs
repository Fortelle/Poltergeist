using System;
using System.Collections.Generic;

namespace Poltergeist.Automations.Processors;

public sealed class ProcessReport
{
    public string MacroName { get; set; }

    public string ProcessId { get; set; }

    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }

    public EndReason EndReason { get; set; }

    public string Summary { get; set; }

    public Dictionary<string, string> Extra { get; set; }

    public ProcessReport()
    {
        Extra = new();
    }

    public void Add(string key, string value)
    {
        Extra.Add(key, value);
    }
}
