using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Macros;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public class MacroGroupViewModel : ObservableRecipient
{
    public ICommand ChooseCommand { get; }

    private MacroBase _selectedMacro;
    public MacroBase SelectedMacro
    {
        get => _selectedMacro;
        set => SetProperty(ref _selectedMacro, value);
    }

    public MacroGroupViewModel(string groupname)
    {
        var manager = App.GetService<MacroManager>();
        //Collection = new AdvancedCollectionView(manager.Macros, true)
        //{
        //    Filter = x =>
        //    {
        //        var group = ((MacroData)x).Category ?? "";
        //        return group == groupname;
        //    }
        //};

        ChooseCommand = new RelayCommand(Choose);
    }

    private void Choose()
    {
        //var process = App.GetService<MacroExecuter>();
        //process.Load(SelectedMacro);
    }

}
