using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Modules.Navigation;
using Poltergeist.Tests.TestTasks;

namespace Poltergeist.Tests.UITests;

public partial class TestTaskViewModel : ObservableRecipient
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public TestTask TestTask { get; set; }

    public TestTaskViewModel(TestTask task)
    {
        TestTask = task;

        Title = task.Title;
    }

    [RelayCommand]
    public async Task Execute()
    {
        if (TestTask.Execute is not null)
        {
            TestTask.Execute.Invoke();
        }
        else if(TestTask.ExecuteAsync is not null)
        {
            await TestTask.ExecuteAsync.Invoke();
        }
        else if (TestTask.PageInfo is not null)
        {
            PoltergeistApplication.TryEnqueue(() =>
            {
                PoltergeistApplication.GetService<NavigationService>().NavigateTo(TestTask.PageInfo);
            });
        }

        await Task.CompletedTask;
    }
}
