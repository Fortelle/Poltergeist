﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Macros;
using Poltergeist.Services;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class MacroGroupPage : Page
{
    public MacroGroupPage()
    {
        InitializeComponent();
    }

    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if(((FrameworkElement)e.OriginalSource).DataContext is MacroBase macro)
        {
            var mng = App.GetService<MacroManager>();
            mng.Set(macro);
        }
    }

    private void MacroConfigControl_ItemUpdated(object sender, System.EventArgs e)
    {
        var group = (MacroGroup)DataContext;
        group.SaveOptions();

        App.ShowFlyout("Options saved");
    }
}
