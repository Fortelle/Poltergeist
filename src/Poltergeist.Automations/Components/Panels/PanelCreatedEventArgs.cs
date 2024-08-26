namespace Poltergeist.Automations.Components.Panels;

public class PanelCreatedEventArgs(PanelModel item) : EventArgs
{
    public PanelModel Item => item;
}
