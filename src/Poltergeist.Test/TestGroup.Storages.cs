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

        UserOptions =
        {
            new ChoiceOption<ParameterSource>("ParameterSource", new[]{
                ParameterSource.Macro,
                ParameterSource.Group,
                ParameterSource.Global
            },
            ParameterSource.Macro),
        },

        Execute = (args) =>
        {
            var source = args.Processor.Options.Get<ParameterSource>("ParameterSource");

            var localStorage = args.Processor.GetService<LocalStorageService>();
            var history = localStorage.Get("history", Array.Empty<int>(), source);

            args.Outputer.NewGroup("Last three results:");
            foreach (var x in history.Take(3))
            {
                args.Outputer.Write(x.ToString());
            }

            var value = new Random().Next();
            args.Outputer.NewGroup("Current result:");
            args.Outputer.Write(value.ToString());

            history = history.Append(value).TakeLast(3).ToArray();
            localStorage.Set("history", history, source);
        },
    };


    [AutoLoad]
    public BasicMacro FileStorageMacro = new("test_filestorage")
    {
        Title = "FileStorage Test",

        Description = "This macro is used for testing the FileStorageService.",

        UserOptions =
        {
            new EnumOption<FileStorageSource>("FileStorageSource", FileStorageSource.Macro),
        },

        Execute = (args) =>
        {
            var source = args.Processor.Options.Get<FileStorageSource>("FileStorageSource");

            var fileStorage = args.Processor.GetService<FileStorageService>();
            var history = fileStorage.Get<int[]>("history.json", source);
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
            fileStorage.Set("history.json", history, source);
        },
    };
}
