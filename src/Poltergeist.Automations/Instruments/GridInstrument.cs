using System;
using System.Collections.Generic;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Instruments;

public class GridInstrument : ColorInstrumentModel<GridInstrumentControl, ListInstrumentViewModel>
{
    public int Columns { get; set; }
    public int Size { get; set; }

    public event Action<int, GridInstrumentItem> Changed;
    public List<GridInstrumentItem> Presets = new();

    public GridInstrument(MacroProcessor processor) : base(processor)
    {
        Size = 32;
    }

    public void SetPlaceholders(int count)
    {
        for(var i = 0; i < count; i++)
        {
            Add(new(ProgressStatus.Idle));
        }
    }

    public void Add(GridInstrumentItem item)
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

    public void Update(int index, GridInstrumentItem item)
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
        return new GridInstrumentViewModel(this);
    }
}
