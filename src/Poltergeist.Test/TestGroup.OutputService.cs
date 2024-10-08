﻿using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{

    [AutoLoad]
    public BasicMacro OutputServiceTest = new("test_outputservice")
    {
        Title = "OutputService Test",
        Description = "This macro is used for testing the OutputService.",

        IsSingleton = true,

        Execute = (args) =>
        {
            var values = Enum.GetValues<OutputLevel>();

            args.Outputer.NewGroup($"Output group 1:");
            foreach (var value in values)
            {
                args.Outputer.Write(value, $"This is a <{value}> message.");
            }

            args.Outputer.NewGroup($"Output group 2:");
            foreach (var value in values)
            {
                args.Outputer.Write(value, $"This is a <{value}> message.");
            }
        }
    };

}
