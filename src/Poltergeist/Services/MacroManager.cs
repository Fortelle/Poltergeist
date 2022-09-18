using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Poltergeist.Automations.Macros;
using Poltergeist.Models;
using Poltergeist.ViewModels;

namespace Poltergeist.Services;

public class MacroManager
{
    //public readonly List<Type> MacroTypes = new();
    public readonly List<MacroBase> Macros = new();
    public readonly List<MacroGroup> Groups = new();

    private readonly string MacroFolder;
    private readonly string SharedFolder;

    public MacroBase CurrentMacro { get; set; }
    public bool IsRunning { get; set; }

    public MacroManager(PathService path)
    {
        MacroFolder = Path.Combine(path.DocumentFolder, "Macros");
        SharedFolder = Path.Combine(path.DocumentFolder, "Shared");
    }

    public MacroBase GetMacro(string name)
    {
        return Macros.FirstOrDefault(x => x.Name == name);
    }

    public void AddMacro(MacroBase macro)
    {
        macro.PrivateFolder = Path.Combine(MacroFolder, macro.Name);
        macro.SharedFolder = SharedFolder;
        Macros.Add(macro);
    }

    public void Set(MacroBase macro)
    {
        if (IsRunning && CurrentMacro != macro)
        {
            App.ShowFlyout("Another macro is already running.");
            return;
        }

        var nav = App.GetService<NavigationService>();
        nav.Navigate("console");

        if (CurrentMacro != macro)
        {
            macro.Initialize();

            var exe = App.GetService<MacroConsoleViewModel>();
            exe.Load(macro);

            CurrentMacro = macro;
        }
    }

    public void Toggle()
    {
        if (CurrentMacro is null)
        {
            return;
        }

        var exe = App.GetService<MacroConsoleViewModel>();

        if (!IsRunning)
        {
            exe.Start();
        }
        else
        {
            exe.Stop();
        }
    }

}
