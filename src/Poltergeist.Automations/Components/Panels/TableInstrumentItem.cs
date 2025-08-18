using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class TableInstrumentItem
{
    public int? Index { get; set; }

    public string? Glyph { get; set; }
    public string? Text { get; set; }
    public string? Emoji { get; set; }

    public string? Tooltip { get; set; }

    public string? TemplateKey { get; set; }

    public ThemeColor? Color { get; set; }

    public TableInstrumentItem()
    {
    }

    public TableInstrumentItem(string templateKey)
    {
        TemplateKey = templateKey;
    }
}



public abstract class TableInstrument<T> : InstrumentModel//, IGridInstrumentModel
    where T : TableInstrumentItem, new()
{
    public ObservableCollection<TableInstrumentItem> Items { get; } = new();
    public Dictionary<string, TableInstrumentItem> Templates = new();
    public T? PlaceholderTemplate { get; }
    public int? MaximumColumns { get; set; }
    public int? IconSize { get; set; }
    public int? IconWidth { get; set; }
    public int? IconHeight { get; set; }

    private readonly List<TableInstrumentItem> Buffer = new();

    public TableInstrument(MacroProcessor processor) : base(processor)
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

    private void Set(int index, TableInstrumentItem item, bool shouldUpdate = false)
    {
        if (!string.IsNullOrEmpty(item.TemplateKey) && Templates.TryGetValue(item.TemplateKey, out var template))
        {
            item.TemplateKey = null;
            TableInstrument<T>.ApplyTemplate(item, template);
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
                TableInstrument<T>.ApplyTemplate(item, Buffer[index]);
            }

            Buffer[index] = item;
            Items[index] = item;
        }

    }

    private static void ApplyTemplate(TableInstrumentItem item, TableInstrumentItem template)
    {
        item.Index ??= template.Index;
        item.Tooltip ??= template.Tooltip;
        item.Text ??= template.Text;
        item.Emoji ??= template.Emoji;
        item.Color ??= template.Color;
        item.Glyph ??= template.Glyph;
    }

}

public class TableInstrument(MacroProcessor processor) : TableInstrument<TableInstrumentItem>(processor)
{
}
