using System;
using System.IO;
using System.Reflection;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Automations.Macros;

internal static class ActionHelper
{
    public static readonly MacroAction OpenLocalFolder = new()
    {
        Text = "Open macro folder",
        Description = "Opens the macro folder in Windows Explorer.",
        Glyph = "\uED25",
        Execute = args =>
        {
            if (!Directory.Exists(args.Macro.PrivateFolder)) return;
            System.Diagnostics.Process.Start("explorer.exe", args.Macro.PrivateFolder);
        },
    };

    public static readonly MacroAction CreateShortcut = new()
    {
        Text = "Create shortcut",
        Description = "Creates a desktop shortcut (*.lnk) for launching the macro directly.",
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
                Title = "Create shortcut",
                Type = DialogType.OkCancel,
                Text = path,
                Inputs = new[]
                {
                    new BoolOption("autostart")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = "Run the macro on startup",
                    },
                    new BoolOption("autoclose")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = "Exit the application on complete",
                    },
                    new BoolOption("singlemode")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = "Use the single macro layout",
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
