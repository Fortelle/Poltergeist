using System;

namespace Poltergeist.Automations.Components.Panels;

public class PanelCreatedEventArgs : EventArgs
{
    public PanelModel Item { get; set; }

    public PanelCreatedEventArgs(PanelModel item)
    {
        Item = item;
    }
}
