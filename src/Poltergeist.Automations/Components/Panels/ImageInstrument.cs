using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public class ImageInstrument : InstrumentModel
{
    public int? MaximumColumns { get; set; }

    public ObservableCollection<ImageInstrumentItem> Items = new();

    public ImageInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(ImageInstrumentItem item)
    {
        Processor.RaiseAction(() =>
        {
            Items.Add(item);
        });
    }

    public void Update(int index, ImageInstrumentItem item)
    {
        if (index == -1 || index >= Items.Count)
        {
            Add(item);
        }
        else if (index < Items.Count)
        {
            Processor.RaiseAction(() =>
            {
                Items[index] = item;
            });
        }
    }
}
