using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public BasicMacro LocalStorageMacro = new("test_localstorage")
    {
        Title = "LocalStorage Test",

        Description = "This macro is used for testing the LocalStorageService.",

        IsSingleton = true,

        UserOptions =
        {
            new BoolOption("IsGlobal"),
        },

        Execute = (args) =>
        {
            var isGlobal = args.Processor.Options.Get<bool>("IsGlobal");

            var localStorage = args.Processor.GetService<LocalStorageService>();
            var history = isGlobal
                ? localStorage.GlobalGet("history", Array.Empty<int>())
                : localStorage.Get("history", Array.Empty<int>());

            args.Outputer.NewGroup("Last three results:");
            foreach (var x in history.Take(3))
            {
                args.Outputer.Write(x.ToString());
            }

            var value = new Random().Next();
            args.Outputer.NewGroup("Current result:");
            args.Outputer.Write(value.ToString());

            history = history.Append(value).TakeLast(3).ToArray();
            if (isGlobal)
            {
                localStorage.GlobalSet("history", history);
            }
            else
            {
                localStorage.Set("history", history);
            }
        },
    };


    [AutoLoad]
    public BasicMacro FileStorageMacro = new("test_filestorage")
    {
        Title = "FileStorage Test",

        Description = "This macro is used for testing the FileStorageService.",

        IsSingleton = true,

        UserOptions =
        {
            new BoolOption("IsGlobal", false),
        },

        Execute = (args) =>
        {
            var isGlobal = args.Processor.Options.Get<bool>("IsGlobal");

            var fileStorage = args.Processor.GetService<FileStorageService>();
            var history = fileStorage.Get<int[]>("history.json", isGlobal);
            history ??= Array.Empty<int>();

            args.Outputer.NewGroup("Last three results:");
            foreach (var x in history.Take(3))
            {
                args.Outputer.Write(x.ToString());
            }

            var value = new Random().Next();
            args.Outputer.NewGroup("Current result:");
            args.Outputer.Write(value.ToString());

            history = history.Append(value).TakeLast(3).ToArray();
            fileStorage.Set("history.json", history, isGlobal);
        },
    };
}
