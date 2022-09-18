using System;
using System.Collections.Generic;
using System.Linq;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Panels;

public class PanelService : MacroService
{
    private List<IOutputPanelModel> Panels = new();

    public PanelService(MacroProcessor processor) : base(processor)
    {
        processor.Hooks.Register("ui_ready", UpdateUI);
    }

    private void UpdateUI(object[] args)
    {
        Processor.Hooks.Raise("ui_panel_ready");
    }

    public T Create<T>(Action<T> config)
        where T : class, IOutputPanelModel
    {
        var panel = Processor.GetService<T>();
        config(panel);

        if (Panels.Any(x=>x.Key == panel.Key))
        {
            throw new ArgumentException("A panel with the same key already exists.");
        }

        Panels.Add(panel);

        var args = new PanelCreatedEventArgs(panel);
        Processor.RaiseEvent(MacroEventType.PanelCreated, args);

        return panel;
    }
}
