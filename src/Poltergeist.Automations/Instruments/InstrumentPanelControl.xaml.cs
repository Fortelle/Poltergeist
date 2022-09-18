using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Poltergeist.Automations.Instruments;

public partial class InstrumentPanelControl : UserControl
{
    public InstrumentPanelControl()
    {
        InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
    }

    //private void ContentControl_Loaded(object sender, RoutedEventArgs e)
    //{
    //    var control = sender as ContentControl;

    //    switch (control.DataContext)
    //    {
    //        case IInstrumentModel x:
    //            var ctl = x.CreateControl();
    //            control.Content = ctl;
    //            return;
    //    }
    //}

}
