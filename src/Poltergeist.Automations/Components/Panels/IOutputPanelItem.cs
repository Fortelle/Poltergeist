namespace Poltergeist.Automations.Components.Panels;

public interface IOutputPanelModel
{
    string Key { get; set; }
    string Header { get; set; }
    object CreateControl();
    object CreateViewModel();
}
