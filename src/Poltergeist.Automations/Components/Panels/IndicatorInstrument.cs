using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public class IndicatorInstrument(MacroProcessor processor) : InstrumentModel(processor)
{
    public Dictionary<string, IndicatorInstrumentItem[]> ItemTemplates { get; } = new();

    public ObservableCollection<IndicatorInstrumentItem> Items { get; } = new();

    public int? IconSize { get; set; }

    private readonly List<IndicatorInstrumentItem> Buffer = new();

    public void Add(string itemKey, IndicatorInstrumentItem[] templates)
    {
        Add(itemKey, templates, 0);
    }

    public void Add(string itemKey, IndicatorInstrumentItem[] templates, int motionIndex)
    {
        ItemTemplates.Add(itemKey, templates);

        Buffer.Add(templates[motionIndex]);
        Items.Add(templates[motionIndex]);
    }

    public void Add(string itemKey, IndicatorInstrumentItem[] templates, string motionKey)
    {
        var motionIndex = Array.FindIndex(templates, x => x.PatternKey == motionKey);
        Add(itemKey, templates, motionIndex);
    }

    public void Switch(string itemKey, string motionKey)
    {
        var itemIndex = ItemTemplates.Keys.ToList().IndexOf(itemKey);
        if (itemIndex < 0)
        {
            throw new KeyNotFoundException(nameof(itemKey));
        }
        var motionItem = ItemTemplates[itemKey].FirstOrDefault(x => x.PatternKey == motionKey);
        if (motionItem is null)
        {
            throw new KeyNotFoundException(nameof(motionKey));
        }

        Items[itemIndex] = motionItem;
    }
}
