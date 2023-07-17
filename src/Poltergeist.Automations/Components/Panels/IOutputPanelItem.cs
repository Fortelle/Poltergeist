using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.Automations.Components.Panels;

public interface IOutputPanelModel
{
    public string Key { get; set; }
    public string Header { get; set; }
    public UserControl CreateControl();
    public object CreateViewModel();
}
