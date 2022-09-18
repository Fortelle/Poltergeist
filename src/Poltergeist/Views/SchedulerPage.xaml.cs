using System.Windows.Controls;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class SchedulerPage : Page
{
    public SchedulerViewModel ViewModel
    {
        get;
    }

    public SchedulerPage()
    {
        ViewModel = App.GetService<SchedulerViewModel>();
        InitializeComponent();
    }
}
