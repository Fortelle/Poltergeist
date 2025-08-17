using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Processors;

public class ProcessorReport : SerializableParameterValueCollection
{
    public ProcessorReport()
    {

    }

    public ProcessorReport(IEnumerable<KeyValuePair<string, object?>> items)
    {
        foreach (var kvp in items)
        {
            Add(kvp.Key, kvp.Value);
        }
    }
}
