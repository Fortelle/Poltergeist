using System.Reflection;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros;

internal static class ActionHelper
{
    public static readonly MacroAction OpenLocalFolder = new()
    {
        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/OpenMacroFolder_Title"),
        Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/OpenMacroFolder_Description"),
        Icon = "\uED25",
        Execute = args =>
        {
            if (!args.Environments.TryGetValue("private_folder", out var privateFolder))
            {
                return;
            }
                
            if (!Directory.Exists((string)privateFolder!))
            {
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", (string)privateFolder);
        },
    };

    public static readonly MacroAction CreateShortcut = new()
    {
        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Title"),
        Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Description"),
        Icon = "\uE8E5",
        Execute = async args =>
        {
            var savedialog = new FileSaveModel()
            {
                SuggestedFileName = $"{args.Macro.Key}.lnk",
                Filters = new()
                {
                    ["Desktop shortcut(*.lnk)"] = [
                        ".lnk"
                    ]
                }
            };
            await InteractionService.UIShowAsync(savedialog);
            var path = savedialog.FileName;

            if (path is null)
            {
                return;
            }

            var optiondialog = new InputDialogModel()
            {
                Title = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Dialog_Title"),
                Text = path,
                Inputs = [
                    new BoolOption("autostart")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Dialog_AutoStart"),
                    },
                    new BoolOption("autoclose")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Dialog_AutoClose"),
                    },
                    new BoolOption("singlemode")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Dialog_SingleMode"),
                    },
                ],
            };
            await InteractionService.UIShowAsync(optiondialog);
            if (optiondialog.Result != DialogResult.Ok)
            {
                return;
            }

            var autostart = (bool)optiondialog.Values![0]!;
            var autoclose = (bool)optiondialog.Values![1]!;
            var singlemode = (bool)optiondialog.Values![2]!;

            var arguments = $"--macro={args.Macro.Key}";
            if (autostart)
            {
                arguments += " --autostart";
            }
            if (autoclose)
            {
                arguments += " --autoclose";
            }
            if (singlemode)
            {
                arguments += " --singlemode";
            }

            var wshShell = new IWshRuntimeLibrary.WshShell();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(path);
            shortcut.TargetPath = Environment.ProcessPath;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = Environment.CurrentDirectory;
            shortcut.IconLocation = Assembly.GetAssembly(typeof(MacroBase))!.Location;
            shortcut.Save();
            if (((MacroBase)args.Macro).RequiresAdmin)
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(21, SeekOrigin.Begin);
                fs.WriteByte(0x22);
                fs.Flush();
            }
        },
    };


}
