using System.Diagnostics;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ActionExample : UnrunnableMacro
{
    public ActionExample() : base()
    {
        Title = "Macro Actions";

        Category = "Browsers";

        Description = "This example shows how to create the macro actions.";

        Actions.Add(new SyncAction()
        {
            Text = "Sync method",
            Description = "A synchronous method that takes 3 seconds to complete.",
            Execute = (context) =>
            {
                Thread.Sleep(3000);
                context.Message = "complete";
            },
        });

        Actions.Add(new AsyncAction()
        {
            Text = "Async method",
            Description = "Popups a progress dialog and executes an asynchronous method.",
            ExecuteAsync = async (context) =>
            {
                await Task.Delay(3000, context.CancellationToken);
                context.Message = "complete";
            },
        });

        Actions.Add(new UriAction()
        {
            Text = "Uri example",
            Description = "Opens https://www.google.com via browser.",
            Uri = @"https://www.google.com",
        });

        Actions.Add(new UriAction()
        {
            Text = "Uri example",
            Description = @"Opens C:\ via file explorer.",
            Uri = @"C:\",
        });

        Actions.Add(new ExternalProcessAction()
        {
            Text = "External process example",
            Description = @"Opens a powershell window and executes a command.",
            GetStartInfo = (context) =>
            {
                return new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = "-noexit echo HelloWorld"
                };
            },
        });

        Actions.Add(new TerminalAction()
        {
            Text = "Terminal page example",
            Description = "Opens a new terminal tab and executes codes.",
            ExecuteAsync = async (context) =>
            {
                var max = 100;
                for (var i = 0; i < max; i++)
                {
                    context.UpdateProgress(1.0 * i / max, $"{i} / {max}");
                    context.Log($"{i}");
                    await Task.Delay(100, context.CancellationToken);
                }
                context.UpdateProgress(1, $"{max} / {max}");
                context.Log($"done.");
            },
        });
    }
}
