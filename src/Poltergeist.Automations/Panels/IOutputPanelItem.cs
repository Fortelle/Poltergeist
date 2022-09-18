using System;
using System.Windows.Controls;

namespace Poltergeist.Automations.Panels;

public interface IOutputPanelModel
{
    public string Key { get; set; }
    public string Header { get; set; }
    public UserControl CreateControl();
    public object CreateViewModel();
}
