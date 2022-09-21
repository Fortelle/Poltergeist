using System.Collections.Generic;
using System.Linq;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.ViewModels;

namespace Poltergeist.Services;

public class MacroManager
{
    public List<MacroBase> Macros { get; } = new();
    public List<MacroGroup> Groups { get; } = new();
    public MacroOptions GlobalOptions { get; set; } = new();

    public MacroBase CurrentMacro { get; set; }
    public bool IsRunning { get; set; }

    private PathService PathService;


    public MacroManager(PathService path)
    {
        PathService = path;
    }

    public MacroBase GetMacro(string name)
    {
        return Macros.FirstOrDefault(x => x.Name == name);
    }

    public void AddMacro(MacroBase macro)
    {
        macro.PrivateFolder = PathService.GetMacroFolder(macro);
        macro.SharedFolder = PathService.SharedFolder;
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
        GlobalOptions.Load(filepath);
    }

    public void SaveGlobalOptions()
    {
        var filepath = PathService.GlobalMacroOptionsFile;
        GlobalOptions.Save(filepath);
    }

}
