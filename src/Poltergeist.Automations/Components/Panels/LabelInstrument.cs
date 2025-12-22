using System.Collections.ObjectModel;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public class LabelInstrument : InstrumentModel
{
    public ObservableCollection<LabelInstrumentItem> Items { get; } = new();
    public Dictionary<string, LabelInstrumentItem> Templates = new();

    public int? MaximumColumns { get; set; }

    private readonly List<LabelInstrumentItem> Buffer = new();

    public LabelInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(LabelInstrumentItem item)
    {
        if (!string.IsNullOrEmpty(item.TemplateKey) && Templates.TryGetValue(item.TemplateKey, out var template))
        {
            item.TemplateKey = null;
            ApplyTemplate(item, template);
        }

        Buffer.Add(item);
        Items.Add(item);
    }

    public void Update(LabelInstrumentItem item)
    {
        var index = Items.Index().First(x => x.Item.Key == item.Key).Index;

        if (!string.IsNullOrEmpty(item.TemplateKey) && Templates.TryGetValue(item.TemplateKey, out var template))
        {
            item.TemplateKey = null;
            ApplyTemplate(item, template);
        }

        ApplyTemplate(item, Buffer[index]);

        Buffer[index] = item;
        Items[index] = item;
    }

    private static void ApplyTemplate(LabelInstrumentItem item, LabelInstrumentItem template)
    {
        item.Tooltip ??= template.Tooltip;
        item.Text ??= template.Text;
        item.Label ??= template.Label;
        item.Color ??= template.Color;
        item.Icon ??= template.Icon;
    }
}
