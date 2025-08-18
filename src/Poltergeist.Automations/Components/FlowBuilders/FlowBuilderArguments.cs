using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Components.FlowBuilders;

public class FlowBuilderExecutionArguments
{
    public event Action<ProgressInstrumentInfo>? Updated;

    public void Update(ProgressInstrumentInfo item)
    {
        Updated?.Invoke(item);
    }
}
