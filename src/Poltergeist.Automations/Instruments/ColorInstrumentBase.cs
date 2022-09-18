using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Poltergeist.Automations.Processors;
using Poltergeist.Common;

namespace Poltergeist.Automations.Instruments;

public abstract class ColorInstrumentModel<TView, TViewModel> : InstrumentModel<TView, TViewModel>
     where TView : UserControl, new()
{
    public readonly Dictionary<string, ThemeColor> Colors = new();

    public ColorInstrumentModel(MacroProcessor processor) : base(processor)
    {
        Colors.Add(ProgressStatus.Idle.ToString(), ThemeColors.Disabled);
        Colors.Add(ProgressStatus.Busy.ToString(), ThemeColors.Busy);
        Colors.Add(ProgressStatus.Succeeded.ToString(), ThemeColors.Succeeded);
        Colors.Add(ProgressStatus.Failed.ToString(), ThemeColors.Failed);
    }

    //public void AddColor(string name, Color backcolor)
    //{
    //    var forecolor = Color.FromRgb((byte)(backcolor.R * .4), (byte)(backcolor.G * .4), (byte)(backcolor.B * .4));

    //    AddColor(name, backcolor, forecolor);
    //}

    //public void AddColor(string name, Color backcolor, Color forecolor)
    //{
    //    BackColors.Add(name, backcolor);
    //    ForeColors.Add(name, forecolor);
    //}
}
