using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public abstract class TileInstrument<T> : InstrumentModel, ITileInstrumentModel
    where T : TileInstrumentItem, new()
{
    public ObservableCollection<TileInstrumentItem> Items { get; } = new();
    public Dictionary<string, TileInstrumentItem> Templates = new();
    public T? PlaceholderTemplate { get; }
    public int? IconSize { get; set; }
    public int? IconWidth { get; set; }
    public int? IconHeight { get; set; }

    private readonly List<TileInstrumentItem> Buffer = new();

    public TileInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(T item)
    {
        Set(-1, item, false);
    }

    public void AddPlaceholders(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var item = new T();
            if (PlaceholderTemplate is not null)
            {
                ApplyTemplate(item, PlaceholderTemplate);
            }
            Add(item);
        }
    }

    public void AddPlaceholders(int count, T template)
    {
        for (var i = 0; i < count; i++)
        {
            var item = new T();
            ApplyTemplate(item, template);
            Add(item);
        }
    }

    public void Update(int index, T item)
    {
        Set(index, item, true);
    }

    public void Update(string key, T item)
    {
        var index = Items.FindIndex(x => x.Key == key);
        if (index == -1)
        {
            throw new KeyNotFoundException();
        }
        Set(index, item, true);
    }

    public void Override(int index, T item)
    {
        Set(index, item, false);
    }

    public void Override(string key, T item)
    {
        var index = Items.FindIndex(x => x.Key == key);
        if (index == -1)
        {
            throw new KeyNotFoundException();
        }
        Set(index, item, false);
    }

    private void Set(int index, TileInstrumentItem item, bool shouldUpdate)
    {
        if (!string.IsNullOrEmpty(item.TemplateKey) && Templates.TryGetValue(item.TemplateKey, out var template))
        {
            item.TemplateKey = null;
            TileInstrument<T>.ApplyTemplate(item, template);
        }

        if (index == -1)
        {
            index = Buffer.Count;
        }

        if (index > Buffer.Count)
        {
            AddPlaceholders(index - Buffer.Count);
        }

        if (index == -1 || index >= Buffer.Count)
        {
            Buffer.Add(item);
            Items.Add(item);
        }
        else
        {
            if (shouldUpdate && Buffer[index] is not null)
            {
                TileInstrument<T>.ApplyTemplate(item, Buffer[index]);
            }

            Buffer[index] = item;
            Items[index] = item;
        }
    }

    private static void ApplyTemplate(TileInstrumentItem item, TileInstrumentItem template)
    {
        item.Key ??= template.Key;
        item.Tooltip ??= template.Tooltip;
        item.Icon ??= template.Icon;
        item.Color ??= template.Color;
    }
}

public class TileInstrument(MacroProcessor processor) : TileInstrument<TileInstrumentItem>(processor)
{
}
