using System;
using System.Collections.Generic;
using System.Linq;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
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
        return Macros.FirstOrDefault(x => x.Name == name);
    }

    public bool Set(string name, LaunchReason? reason = null)
    {
        var macro = GetMacro(name);
        if (macro == null) return false;
        return Set(macro, reason);
    }

    public bool Set(IMacroBase macro, LaunchReason? reason = null)
    {
        if (IsRunning && CurrentMacro != macro)
        {
            App.ShowFlyout("Another macro is already running.");
            return false;
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

        if (reason.HasValue)
        {
            TryStart(reason.Value);
        }

        return true;
    }

    public void Toggle()
    {
        if (!IsRunning)
        {
            TryStart(LaunchReason.ByUser);
        }
        else
        {
            TryStop();
        }
    }

    private void TryStart(LaunchReason reason)
    {
        if (CurrentMacro is null) return;
        if (IsRunning) return;
        var consoleVM = App.GetService<MacroConsoleViewModel>();
        consoleVM.Start(reason);
    }

    private void TryStop()
    {
        if (CurrentMacro is null) return;
        if (!IsRunning) return;
        var consoleVM = App.GetService<MacroConsoleViewModel>();
        consoleVM.Stop();
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
