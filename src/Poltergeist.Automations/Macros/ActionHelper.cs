using System.Reflection;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Macros;

internal static class ActionHelper
{
    public static readonly MacroAction OpenLocalFolder = new()
    {
        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/OpenMacroFolder_Title"),
        Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/OpenMacroFolder_Description"),
        Glyph = "\uED25",
        Execute = args =>
        {
            if (!Directory.Exists(args.Macro.PrivateFolder))
            {
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", args.Macro.PrivateFolder);
        },
    };

    public static readonly MacroAction CreateShortcut = new()
    {
        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Title"),
        Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Description"),
        Glyph = "\uE8E5",
        Execute = async args =>
        {
            var savedialog = new FileSaveModel()
            {
                SuggestedFileName = $"{args.Macro.Key}.lnk",
                Filters = new()
                {
                    ["Desktop shortcut(*.lnk)"] = new[] {
                        ".lnk"
                    }
                }
            };
            await InteractionService.UIShowAsync(savedialog);
            var path = savedialog.FileName;

            if (path is null)
            {
                return;
            }

            var optiondialog = new DialogModel()
            {
                Title = ResourceHelper.Localize("Poltergeist.Automations/Resources/CreateShortcut_Dialog_Title"),
                Type = DialogType.OkCancel,
                Text = path,
                Inputs = new[]
                {
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
                },
            };
            await InteractionService.UIShowAsync(optiondialog);
            if (optiondialog.Result != DialogResult.Ok)
            {
                return;
            }

            var autostart = (bool)optiondialog.Values![0];
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
            if (args.Macro.RequiresAdmin)
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(21, SeekOrigin.Begin);
                fs.WriteByte(0x22);
                fs.Flush();
            }
        },
    };


}
