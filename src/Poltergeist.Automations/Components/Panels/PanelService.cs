using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Panels;

public class PanelService : KernelService
{
    private readonly List<PanelModel> Panels = new();

    public PanelService(MacroProcessor processor) : base(processor)
    {
    }

    public PanelModel Create(PanelModel panel)
    {
        if (Panels.Any(x => x.Key == panel.Key))
        {
            throw new ArgumentException("A panel with the same key already exists.");
        }

        Panels.Add(panel);

        var args = new PanelCreatedEventArgs(panel);
        Processor.RaiseEvent(ProcessorEvent.PanelCreated, args);

        return panel;
    }

}
