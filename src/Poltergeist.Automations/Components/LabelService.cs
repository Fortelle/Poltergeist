using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class LabelService : MacroService
{
    private readonly LabelInstrument LabelInstrument;

    public LabelService(
        MacroProcessor processor,
        DashboardService dashboardService,
        LabelInstrument labelInstrument
        ) : base(processor)
    {
        LabelInstrument = labelInstrument;
        labelInstrument.MaximumColumns = 4;
        dashboardService.Add(labelInstrument);
    }

    public void Add(LabelInstrumentItem item)
    {
        LabelInstrument.Add(item);
    }

    public void Update(LabelInstrumentItem item)
    {
        LabelInstrument.Update(item);
    }
}
