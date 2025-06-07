using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public abstract class GridInstrument<T> : InstrumentModel, IGridInstrumentModel
    where T : GridInstrumentItem, new()
{
    public ObservableCollection<GridInstrumentItem> Items { get; } = new();
    public Dictionary<string, GridInstrumentItem> Templates = new();
    public T? PlaceholderTemplate { get; }
    public int? MaximumColumns { get; set; }
    public int? IconSize { get; set; }
    public int? IconWidth { get; set; }
    public int? IconHeight { get; set; }

    private readonly List<GridInstrumentItem> Buffer = new();

    public GridInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(T item)
    {
        Set(-1, item);
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

    public void Override(int index, T item)
    {
        Set(index, item);
    }

    private void Set(int index, GridInstrumentItem item, bool shouldUpdate = false)
    {
        if (!string.IsNullOrEmpty(item.TemplateKey) && Templates.TryGetValue(item.TemplateKey, out var template))
        {
            item.TemplateKey = null;
            GridInstrument<T>.ApplyTemplate(item, template);
        }

        if (index == -1)
        {
            item.Index = Buffer.Count;
        }
        else
        {
            item.Index = index;
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
                GridInstrument<T>.ApplyTemplate(item, Buffer[index]);
            }

            Buffer[index] = item;
            Items[index] = item;
        }

    }

    private static void ApplyTemplate(GridInstrumentItem item, GridInstrumentItem template)
    {
        item.Index ??= template.Index;
        item.Tooltip ??= template.Tooltip;
        item.Text ??= template.Text;
        item.Emoji ??= template.Emoji;
        item.Color ??= template.Color;
        item.Glyph ??= template.Glyph;
    }

}

public class GridInstrument(MacroProcessor processor) : GridInstrument<GridInstrumentItem>(processor)
{
}
