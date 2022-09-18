using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Macros;
using Poltergeist.Models;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public class HomeViewModel : ObservableRecipient
{
    public List<MacroGroup> Groups { get; set; }

    public HomeViewModel(MacroManager macroManager)
    {
        Groups = macroManager.Groups; //macroManager.Macros.ToArray();
    }

}
