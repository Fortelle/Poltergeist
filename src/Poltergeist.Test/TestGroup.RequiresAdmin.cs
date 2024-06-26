﻿using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public BasicMacro AdminTest = new("test_admin")
    {
        Title = "RequiresAdmin Test",
        Description = "This macro is used for testing the RequiresAdmin property.",
        IsSingleton = true,

        RequiresAdmin = true,
        Execute = (args) =>
        {
            args.Outputer.Write($"Hello world!");
        }
    };
}
