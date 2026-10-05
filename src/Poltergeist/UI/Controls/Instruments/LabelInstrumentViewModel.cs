using Microsoft.UI.Xaml;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class LabelInstrumentViewModel : IInstrumentViewModel
{
    public string? Title { get; set; }

    public int MaximumColumns { get; set; }

    private readonly LabelHeaderPosition HeaderPosition;

    private readonly LabelTextPosition TextPosition;

    public SynchronizableCollection<LabelInstrumentItem, LabelInstrumentItemViewModel> Items { get; set; }

    public LabelInstrumentViewModel(LabelInstrument model)
    {
        Title = model.Title;
        MaximumColumns = model.MaximumColumns ?? -1;
        HeaderPosition = model.HeaderPosition;
        TextPosition = model.TextPosition;

        Items = new(model.Items, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private LabelInstrumentItemViewModel? ModelToViewModel(LabelInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new LabelInstrumentItemViewModel(item)
        {
            HeaderRow = HeaderPosition switch
            {
                LabelHeaderPosition.TopLeft or LabelHeaderPosition.TopCenter or LabelHeaderPosition.TopRight => 0,
                LabelHeaderPosition.BottomLeft or LabelHeaderPosition.BottomCenter or LabelHeaderPosition.BottomRight => 2,
                LabelHeaderPosition.Hidden => 3,
                _ => throw new NotImplementedException(),
            },
            HeaderDirection = HeaderPosition switch
            {
                LabelHeaderPosition.TopRight or LabelHeaderPosition.BottomRight => FlowDirection.RightToLeft,
                _ => FlowDirection.LeftToRight,
            },
            HeaderAlignment = HeaderPosition switch
            {
                LabelHeaderPosition.TopLeft or LabelHeaderPosition.BottomLeft => HorizontalAlignment.Left,
                LabelHeaderPosition.TopRight or LabelHeaderPosition.BottomRight => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Center,
            },
            TextAlignment = TextPosition switch
            {
                LabelTextPosition.Left => HorizontalAlignment.Left,
                LabelTextPosition.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Center,
            },
        };

        return vm;
    }
}
