﻿using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Common.Structures.Colors;
using Poltergeist.Models;
using Poltergeist.Pages.Macros.Instruments;

namespace Poltergeist.Macros.Instruments;

public class GridInstrumentViewModel : IInstrumentViewModel
{
    public const int DefaultIconSize = 48;

    public string? Title { get; set; }
    public int MaximumColumns { get; set; }
    public int IconWidth { get; set; }
    public int IconHeight { get; set; }

    public SynchronizableCollection<GridInstrumentItem, GridInstrumentItemViewModel> Items { get; set; }

    protected readonly Dictionary<string, SolidColorBrush> ForeColors = new();
    protected readonly Dictionary<string, SolidColorBrush> BackColors = new();
    protected readonly Dictionary<string, string> Glyphs = new();

    public GridInstrumentViewModel(IGridInstrumentModel model)
    {
        Title = model.Title;
        MaximumColumns = (model.MaximumColumns == null || model.MaximumColumns <= 0) ? -1 : model.MaximumColumns.Value;
        IconWidth = model.IconWidth ?? model.IconSize ?? DefaultIconSize;
        IconHeight = model.IconHeight ?? model.IconSize ?? DefaultIconSize;

        Items = new(model.Items, ModelToViewModel, App.MainWindow.DispatcherQueue);
    }

    private GridInstrumentItemViewModel? ModelToViewModel(GridInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new GridInstrumentItemViewModel(item);

        if (item.Color != null)
        {
            var colorset = ThemeColors.Colors[item.Color.Value];
            vm.Foreground = new SolidColorBrush(colorset.Foreground);
            vm.Background = new SolidColorBrush(colorset.Background);
        }

        return vm;
    }
}
