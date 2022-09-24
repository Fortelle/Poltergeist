using System;
using System.Collections.Generic;
using System.Linq;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.ViewModels;

namespace Poltergeist.Services;

public class MacroManager
{
    public List<IMacroBase> Macros { get; } = new();
    public List<MacroGroup> Groups { get; } = new();
    public MacroOptions GlobalOptions { get; set; } = new();

    public IMacroBase CurrentMacro { get; set; }
    public bool IsRunning { get; set; }

    private PathService PathService;


    public MacroManager(PathService path)
    {
        PathService = path;
    }

    public void AddMacro(IMacroBase macro)
    {
        macro.PrivateFolder = PathService.GetMacroFolder(macro);
        macro.SharedFolder = PathService.SharedFolder;
        try
        {
            macro.Initialize();
            Macros.Add(macro);
        } catch (Exception)
        {
        }
    }

    public void AddGroup(MacroGroup group)
    {
        group.GroupFolder = PathService.GetGroupFolder(group);
        group.LoadOptions();

        Groups.Add(group);

        foreach (var macro in group.Macros)
        {
            macro.Group = group;
            AddMacro(macro);
        }
    }

    public IMacroBase GetMacro(string name)
    {
        return Macros.First(x => x.Name == name);
    }

    public void Set(IMacroBase macro)
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




    public void AddGlobalOption(IOptionItem option)
    {
        GlobalOptions.Add(option);
    }

    public void LoadGlobalOptions()
    {
        foreach(var group in Groups)
        {
            group.SetGlobalOptions(GlobalOptions);
        }

        foreach (var module in Macros.SelectMany(x=>x.Modules))
        {
            module.SetGlobalOptions(GlobalOptions);
        }

        var filepath = PathService.GlobalMacroOptionsFile;
        GlobalOptions.Load(filepath, true);
    }

    public void SaveGlobalOptions()
    {
        var filepath = PathService.GlobalMacroOptionsFile;
        GlobalOptions.Save(filepath);
    }

}
