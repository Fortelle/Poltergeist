using System.Text.RegularExpressions;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Input.Windows;

namespace Poltergeist.Test;

public partial class TestGroup
{

    [AutoLoad]
    public class OptionTestMacro : BasicMacro
    {
        private readonly IOptionItem[] CustomOptions = new IOptionItem[]
        {
            new OptionItem<string>("string")
            {
                Category = "Basic",
            },
            new OptionItem<int>("int")
            {
                Category = "Basic",
            },
            new OptionItem<bool>("bool")
            {
                Category = "Basic",
            },
            new EnumOption<DayOfWeek>("enum")
            {
                Category = "Basic",
            },
            new OptionItem<TimeOnly>("TimeOnly")
            {
                Category = "Basic",
            },
            new OptionItem<HotKey>("HotKey")
            {
                Category = "Basic",
            },
            new OptionItem<string[]>("string_array")
            {
                DisplayLabel = "string[]",
                Category = "Basic",
            },

            new TextOption("string_valid")
            {
                Category = "Basic",
                DisplayLabel = "^\\d+$",
                Valid = s => Regex.IsMatch(s, @"^\d+$"),
            },

            new TextOption("string_multiline")
            {
                Category = "Basic",
                Multiline = true,
            },

            new BoolOption("bool_switch")
            {
                Category = "Booleans",
                Mode = BoolOptionMode.ToggleSwitch,
            },
            new BoolOption("bool_checkbox")
            {
                Category = "Booleans",
                Mode = BoolOptionMode.CheckBox,
            },
            new BoolOption("bool_buttons")
            {
                Category = "Booleans",
                Mode = BoolOptionMode.ToggleButtons,
            },
            new BoolOption("bool_leftright")
            {
                Category = "Booleans",
                Mode = BoolOptionMode.LeftRightSwitch,
            },

            new NumberOption<int>("int_numberbox")
            {
                Category = "Numbers",
                Minimum = 0,
                Maximum = 100,
            },
            new NumberOption<int>("int_slider")
            {
                Category = "Numbers",
                Minimum = 0,
                Maximum = 100,
                Layout = NumberOptionLayout.Slider,
            },

            new NumberOption<double>("double_numberbox")
            {
                Category = "Numbers",
                Minimum = 0,
                Maximum = 1,
                StepFrequency = 0.01,
            },
            new NumberOption<double>("double_slider")
            {
                Category = "Numbers",
                Minimum = 0,
                Maximum = 1,
                StepFrequency = 0.01,
                Layout = NumberOptionLayout.Slider,
                ValueFormat = "P0",
            },

            new ChoiceOption<string>("choice_string", new string[] { "Item1", "Item2", "Item3" })
            {
                Category = "Choices",
            },
            new ChoiceOption<int>("choice_int", new int[] { 100, 200, 300 })
            {
                Category = "Choices",
            },

            new ChoiceOption<string>("choice_slider", new string[] { "Item1", "Item2", "Too long item3" }, "Item2")
            {
                Category = "Choices",
                Mode = ChoiceOptionMode.Slider,
            },

            new ChoiceOption<string>("choice_buttons", new string[] { "Item1", "Item2", "Too long item3" }, "Item2")
            {
                Category = "Choices",
                Mode = ChoiceOptionMode.ToggleButtons,
            },

            new PathOption("file_open")
            {
                Category = "Pickers",
                Mode = PathOptionMode.FileOpen
            },
            new PathOption("file_save")
            {
                Category = "Pickers",
                Mode = PathOptionMode.FileSave
            },
            new PathOption("folder_open")
            {
                Category = "Pickers",
                Mode = PathOptionMode.FolderOpen
            },

            new PasswordOption("password")
            {
                Category = "Others",
            },

            new OptionItem<int>("test_option_a", 2)
            {
                Category = "Test",
            },
            new OptionItem<int>("test_option_d", 2)
            {
                Category = "Test",
            },
        };

        public OptionTestMacro() : base("test_macrooptions")
        {
            Title = "Macro Options Test";
            Description = "This macro is used for testing macro options.";

            ShowStatusBar = false;

            foreach (var option in CustomOptions)
            {
                UserOptions.Add(option);
            }

            Execution = (args) =>
            {
                var groups = CustomOptions.GroupBy(x => x.Category);
                foreach (var group in groups)
                {
                    args.Outputer.NewGroup(group.Key!);
                    foreach (var option in group)
                    {
                        var value = args.Processor.GetOption(option.Key, option.BaseType)!;
                        args.Outputer.Write(option.Key, $"{value}");
                    }
                }
            };
        }
    }

}
