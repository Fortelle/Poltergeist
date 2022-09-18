using System;

namespace Poltergeist.Automations.Panels;

public class PanelCreatedEventArgs : EventArgs
{
    public IOutputPanelModel Item;

    public PanelCreatedEventArgs(IOutputPanelModel item)
    {
        Item = item;
    }
}
