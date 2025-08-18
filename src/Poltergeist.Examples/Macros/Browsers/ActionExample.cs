namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ActionExample : UnrunnableMacro
{
    public ActionExample() : base()
    {
        Title = "Macro Actions";

        Category = "Browsers";

        Description = "This example shows how to create the macro actions.";

        Actions.Add(new()
        {
            Text = "Sync method",
            Description = "A synchronous method that takes 3 seconds to complete.",
            Execute = (args) =>
            {
                Thread.Sleep(3000);
                args.Message = "complete";
            },
        });

        Actions.Add(new()
        {
            Text = "Async method",
            Description = "Popups a progress dialog and executes an asynchronous method.",
            ExecuteAsync = async (args) =>
            {
                await Task.Delay(3000);
                args.Message = "complete";
            },
        });

        Actions.Add(new()
        {
            Text = "Cancellable async method",
            Description = "Popups a cancellable progress dialog and executes an asynchronous method.",
            IsCancellable = true,
            ExecuteAsync = async (args) =>
            {
                await Task.Delay(10000, args.CancellationToken);
                args.Message = "complete";
            },
        });
    }

}
