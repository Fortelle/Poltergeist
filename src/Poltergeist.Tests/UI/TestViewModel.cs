using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Tests.TestTasks;

namespace Poltergeist.Tests.UITests;

public partial class TestViewModel : ObservableRecipient
{
    public List<TestTaskViewModel> TestTasks { get; } = new();

    public TestViewModel()
    {

        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(TestTask)))
            {
                var task = (TestTask)Activator.CreateInstance(type)!;
                TestTasks.Add(new TestTaskViewModel(task));
            }
        }

    }
}
