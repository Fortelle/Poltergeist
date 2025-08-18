using Poltergeist.Tests.TestTasks;
using Poltergeist.UI.Controls.Options;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEx;

namespace Poltergeist.Tests.KnownIssues;

/// <seealso cref="PickerOptionControl"/>
// https://github.com/microsoft/WindowsAppSDK/issues/2504
public class AdminPickerIssue : TestTask
{
    public override string Title => "Opening picker dialog as admin will cause crash";

    public override Func<Task>? ExecuteAsync => async () =>
    {
        var picker = new FolderPicker();

        var hWnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(picker, hWnd);

        await picker.PickSingleFolderAsync();
    };
}
