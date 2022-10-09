using System;
using System.Collections.Generic;
using System.ComponentModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Instruments;

public class ListInstrument : ColorInstrumentModel<ListInstrumentControl, ListInstrumentViewModel>, INotifyPropertyChanged
{
    protected void OnPropertyChanged(string propertyName)
    {
        Processor.RaiseAction(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    public event Action<int, ListInstrumentItem> Changed;
    public event PropertyChangedEventHandler PropertyChanged;
    public List<ListInstrumentItem> Presets = new();

    public ListInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(ListInstrumentItem item)
    {
        if (!IsCreated)
        {
            Presets.Add(item);
        }
        else
        {
            Processor.RaiseAction(() =>
            {
                Changed?.Invoke(-1, item);
            });
        }
    }

    public void Update(int index, ListInstrumentItem item)
    {
        if (!IsCreated)
        {
            Presets[index] = item;
        }
        else
        {
            Processor.RaiseAction(() =>
            {
                Changed?.Invoke(index, item);
            });
        }
    }

    public override InstrumentItemViewModel CreateViewModel()
    {
        return new ListInstrumentViewModel(this);
    }

}
