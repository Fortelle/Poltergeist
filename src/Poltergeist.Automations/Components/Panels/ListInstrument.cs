using System.Collections.ObjectModel;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public class ListInstrument<T> : InstrumentModel, IListInstrumentModel
    where T : ListInstrumentItem, new()
{
    public ObservableCollection<ListInstrumentItem> Items { get; } = new();
    public Dictionary<string, ListInstrumentItem> Templates { get; } = new();
    public T? PlaceholderTemplate { get; set; }

    private readonly List<ListInstrumentItem> Buffer = new();
    private bool IsHookRegistered;

    public ListInstrument(MacroProcessor processor) : base(processor)
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
            if(PlaceholderTemplate is not null)
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
        var index = Buffer.FindIndex(x => x?.Key == key);
        Update(index, item);
    }

    public void Override(int index, T item)
    {
        Set(index, item);
    }

    public void Override(string key, T item)
    {
        var index = Buffer.FindIndex(x => x?.Key == key);
        Override(index, item);
    }

    private void Set(int index, ListInstrumentItem item, bool shouldUpdate = false)
    {
        if(!string.IsNullOrEmpty(item.TemplateKey) && Templates.TryGetValue(item.TemplateKey, out var template))
        {
            item.TemplateKey = null;
            ListInstrument<T>.ApplyTemplate(item, template);
        }

        item.InstrumentKey = Key;
        item.MacroKey = MacroKey;
        item.ProcessorId = ProcessorId;
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

        if (item.Buttons?.Length > 0)
        {
            for(var i = 0; i < item.Buttons.Length; i++)
            {
                item.Buttons[i].Argument = new InteractionMessage()
                {
                    ProcessorId = item.ProcessorId,
                    ["instrument_key"] = $"{item.InstrumentKey}",
                    ["item_key"] = $"{item.Key}",
                    ["item_index"] = $"{item.Index}",
                    ["selected_index"] = $"{i}",
                };
            }

            RegisterHook();
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
                ListInstrument<T>.ApplyTemplate(item, Buffer[index]);
            }

            Buffer[index] = item;
            Items[index] = item;
        }

    }

    private void OnButtonClicked(MessageReceivedHook hook)
    {
        var instrumentKey = Key ?? "";
        if (hook.Arguments["instrument_key"] != instrumentKey)
        {
            return;
        }

        var itemIndex = int.Parse(hook.Arguments["item_index"]);
        var selectedIndex = int.Parse(hook.Arguments["selected_index"]);
        Buffer[itemIndex]!.Buttons![selectedIndex].Callback?.Invoke();
    }

    private void RegisterHook()
    {
        if (IsHookRegistered)
        {
            return;
        }

        IsHookRegistered = true;
        var hookService = Processor.GetService<HookService>();
        hookService.Register<MessageReceivedHook>(OnButtonClicked);
    }

    private static void ApplyTemplate(ListInstrumentItem item, ListInstrumentItem template)
    {
        item.Key ??= template.Key;
        item.Index ??= template.Index;
        item.Text ??= template.Text;
        item.Subtext ??= template.Subtext;
        item.Progress ??= template.Progress;
        item.Color ??= template.Color;
        item.Glyph ??= template.Glyph;
        item.Emoji ??= template.Emoji;
        item.Buttons ??= template.Buttons;
        item.TemplateKey ??= template.TemplateKey;
    }
}

public class ListInstrument(MacroProcessor processor) : ListInstrument<ListInstrumentItem>(processor)
{
}
