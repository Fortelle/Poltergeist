using Poltergeist.Automations.Components.Panels;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class ImageInstrumentViewModel : IInstrumentViewModel
{
    public string? Title { get; set; }
    public int MaximumColumns { get; set; }

    public SynchronizableCollection<ImageInstrumentItem, ImageInstrumentItemViewModel> Items { get; set; }

    public ImageInstrumentViewModel(ImageInstrument gi)
    {
        Title = gi.Title;
        MaximumColumns = gi.MaximumColumns ?? -1;

        Items = new(gi.Items, ModelToViewModel, PoltergeistApplication.MainWindow.DispatcherQueue);
    }

    private ImageInstrumentItemViewModel? ModelToViewModel(ImageInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new ImageInstrumentItemViewModel(item);
        item.Dispose();
        return vm;
    }

}
