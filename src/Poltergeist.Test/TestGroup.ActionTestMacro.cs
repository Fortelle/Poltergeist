using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public BasicMacro ActionTestMacro = new("test_action")
    {
        Title = "Action Test",

        Description = "This macro is used for testing macro actions.",

        IsSingleton = true,

        Actions = {
            new()
            {
                Text = "Sync method",
                Execute = (args) =>
                {
                    Thread.Sleep(5000);
                    args.Message = "complete";
                },
            },

            new()
            {
                Text = "Sync method with an exception",
                Execute = (args) =>
                {
                    Thread.Sleep(5000);
                    throw new Exception("error");
                },
            },

            new()
            {
                Text = "Async method",
                ExecuteAsync = async (args) =>
                {
                    await Task.Delay(5000, args.CancellationToken);
                    args.Message = "complete";
                },
            },

            new()
            {
                Text = "Async method with an exception",
                ExecuteAsync = async (args) =>
                {
                    await Task.Delay(5000, args.CancellationToken);
                    throw new Exception("error");
                },
            },

            new()
            {
                Text = "Cancellable async method",
                IsCancellable = true,
                ExecuteAsync = async (args) =>
                {
                    await Task.Delay(5000, args.CancellationToken);
                    args.Message = "complete";
                },
            },
        },
    };

}
