using System.Text.RegularExpressions;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Input.Windows;

namespace Poltergeist.Test;

public partial class TestGroup
{

    [AutoLoad]
    public class OptionTestMacro : BasicMacro
    {
        private readonly IParameterDefinition[] CustomOptions = new IParameterDefinition[]
        {
            new OptionDefinition<string>("string")
            {
                Category = "Basic",
            },
            new OptionDefinition<int>("int")
            {
                Category = "Basic",
            },
            new OptionDefinition<bool>("bool")
            {
                Category = "Basic",
            },
            new EnumOption<DayOfWeek>("enum")
            {
                Category = "Basic",
            },
            new OptionDefinition<TimeOnly>("TimeOnly")
            {
                Category = "Basic",
            },
            new OptionDefinition<HotKey>("HotKey")
            {
                Category = "Basic",
            },
            new OptionDefinition<string[]>("string_array")
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
            new RatingOption("rating")
            {
                Category = "Numbers",
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

            new OptionDefinition<int>("test_option_a", 2)
            {
                Category = "Test",
            },
            new OptionDefinition<int>("test_option_d", 2)
            {
                Category = "Test",
            },

            new OptionDefinition<int>("ReadOnly", 0)
            {
                Category = "Type",
                Status = ParameterStatus.ReadOnly,
            },
            new OptionDefinition<int>("Experimental", 0)
            {
                Category = "Type",
                Status = ParameterStatus.Experimental,
            },
            new OptionDefinition<int>("DevelopmentOnly", 0)
            {
                Category = "Type",
                Status = ParameterStatus.DevelopmentOnly,
            },
            new OptionDefinition<int>("Deprecated", 0)
            {
                Category = "Type",
                Status = ParameterStatus.Deprecated,
            },
        };

        public OptionTestMacro() : base("test_macrooptions")
        {
            Title = "Macro Options Test";
            Description = "This macro is used for testing macro options.";
            IsSingleton = true;
            ShowStatusBar = false;

            foreach (var option in CustomOptions)
            {
                UserOptions.Add(option);
            }

            Execute = (args) =>
            {
                var groups = CustomOptions.GroupBy(x => x.Category);
                foreach (var group in groups)
                {
                    args.Outputer.NewGroup(group.Key!);
                    foreach (var option in group)
                    {
                        var value = args.Processor.Options.Get(option.Key)!;
                        args.Outputer.Write(option.Key, $"{value}");
                    }
                }
            };
        }
    }

}
