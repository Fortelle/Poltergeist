using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Instruments;

public class InstrumentPanel : OutputPanelModel<InstrumentPanelControl, InstrumentPanelViewModel>
{
    public List<IInstrumentModel> Instruments { get; set; } = new();

    public event Action<IInstrumentModel> InstrumentCreated;

    public InstrumentPanel(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(IInstrumentModel instrument)
    {
        if (Instruments.Any(x =>!string.IsNullOrEmpty(x.Key) && x.Key == instrument.Key))
        {
            throw new ArgumentException("An instrument with the same key already exists.");
        }

        Instruments.Add(instrument);

        Processor.RaiseAction(() =>
        {
            InstrumentCreated?.Invoke(instrument);
        });
    }

    public T Get<T>() where T : IInstrumentModel
    {
        return (T)Instruments.FirstOrDefault(x => x is T);
    }

    public T Get<T>(string key) where T : IInstrumentModel
    {
        return (T)Instruments.FirstOrDefault(x => x is T && x.Key == key);
    }

    public override object CreateViewModel()
    {
        return new InstrumentPanelViewModel(this);
    }
}

public class InstrumentPanelViewModel
{
    public ObservableCollection<InstrumentItemViewModel> Instruments { get; set; } = new();


    public InstrumentPanelViewModel(InstrumentPanel model) 
    {
        model.InstrumentCreated += Model_InstrumentCreated;
    }

    private void Model_InstrumentCreated(IInstrumentModel obj)
    {
        var control = obj.CreateControl();
        var vm = obj.CreateViewModel();
        vm.Control = obj.CreateControl();
        control.DataContext = vm;
        Instruments.Add(vm);
    }
}

public class InstrumentItemViewModel
{
    public UserControl Control { get; set; }

    public InstrumentItemViewModel()
    {
    }
}

//<DataTemplate>
//    <ContentControl Content = "{Binding Content}" Margin="0, 16, 0, 0"/>
//</DataTemplate>
