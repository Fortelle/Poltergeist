using System.Diagnostics;
using Poltergeist.Tests.TestTasks;

namespace Poltergeist.Tests.KnownIssues;

public class CloseMainWindowIssue : TestTask
{
    public override string Title => "Process.CloseMainWindow() does not work";

    public override Action? Execute => () =>
    {
        var process = Process.Start("cmd");

        Thread.Sleep(1000);

        var result = process.CloseMainWindow(); // always returns false
    };
}
