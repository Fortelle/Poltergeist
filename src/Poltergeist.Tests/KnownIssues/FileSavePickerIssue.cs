using Poltergeist.Tests.TestTasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEx;

namespace Poltergeist.Tests.KnownIssues;

public class FileSavePickerIssue : TestTask
{
    public override string Title => "FileSavePicker does not support wildcard filter";

    public override Func<Task>? ExecuteAsync => async () =>
    {
        var picker = new FileSavePicker();

        picker.FileTypeChoices.Add("All files", ["."]);

        var hWnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(picker, hWnd);

        await picker.PickSaveFileAsync();
    };
}
