using Microsoft.UI.Xaml.Controls;
using Poltergeist.Tests.UITests;

namespace Poltergeist.Tests.UI;

public sealed partial class TestPage : Page
{
    public TestViewModel ViewModel { get; } = new();

    public TestPage()
    {
        InitializeComponent();
    }
}
