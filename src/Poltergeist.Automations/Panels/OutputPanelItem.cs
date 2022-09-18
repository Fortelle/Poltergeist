using System;
using System.Windows.Controls;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Panels;

public abstract class OutputPanelModel<TView, TViewModel> : MacroService, IOutputPanelModel
    where TView : UserControl, new()
{
    public string Key { get; set; }
    public string Header { get; set; }

    public OutputPanelModel(MacroProcessor processor) : base(processor)
    {
    }

    public virtual UserControl CreateControl()
    {
        return new TView();
    }

    public virtual object CreateViewModel()
    {
        return null;
    }
}
