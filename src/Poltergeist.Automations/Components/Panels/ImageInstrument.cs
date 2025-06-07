using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public class ImageInstrument : InstrumentModel
{
    public int? MaximumColumns { get; set; }

    public ObservableCollection<ImageInstrumentItem> Items = new();

    private List<string> Keys = new();

    public ImageInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(ImageInstrumentItem item)
    {
        Keys.Add(string.Empty);
        Items.Add(item);
    }

    public void Add(string key, ImageInstrumentItem item)
    {
        Keys.Add(key);
        Items.Add(item);
    }

    public void Update(int index, ImageInstrumentItem item)
    {
        if (index == -1 || index >= Items.Count)
        {
            Add(item);
        }
        else if (index < Items.Count)
        {
            Items[index] = item;
        }
    }

    public void Update(string key, ImageInstrumentItem item)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        var index = Keys.IndexOf(key);
        if (index == -1)
        {
            throw new KeyNotFoundException(nameof(key));
        }

        Update(index, item);
    }

    public void Update(int index, string label)
    {
        if (index < 0 && index >= Keys.Count)
        {
            throw new IndexOutOfRangeException(nameof(index));
        }

        var oldItem = Items[index];
        Update(index, new ImageInstrumentItem(new(oldItem.Image), label));
    }

    public void Update(string key, string label)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        var index = Keys.IndexOf(key);
        if (index == -1)
        {
            throw new KeyNotFoundException(nameof(key));
        }

        Update(index, label);
    }

    public void Clear()
    {
        Keys.Clear();
        Items.Clear();
    }

}
