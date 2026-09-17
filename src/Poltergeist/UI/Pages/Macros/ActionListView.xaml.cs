using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Macros;
using Poltergeist.Modules.Macros;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.UI.Pages.Macros;

public sealed partial class ActionListView : UserControl
{
    public static readonly DependencyProperty MacroInstanceProperty = DependencyProperty.RegisterAttached(nameof(MacroInstance), typeof(MacroInstance), typeof(ActionListView), new PropertyMetadata(null));
    public MacroInstance? MacroInstance
    {
        get => (MacroInstance?)GetValue(MacroInstanceProperty);
        set => SetValue(MacroInstanceProperty, value);
    }

    public ActionListView()
    {
        InitializeComponent();
    }

    private void ActionExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroAction action)
        {
            return;
        }

        if (MacroInstance is null)
        {
            return;
        }

        var macro = MacroInstance.Template;

        if (macro is null)
        {
            App.ShowTeachingTip("The macro does not exist.");
            return;
        }

        if (macro.Exception is not null)
        {
            App.ShowTeachingTip("The macro has not been initialized correctly.");
            return;
        }

        var actionIndex = macro.Actions.IndexOf(action);
        if (actionIndex == -1)
        {
            App.ShowTeachingTip("The action is not owned by the macro.");
            return;
        }

        App.GetService<MacroActionService>().Execute(MacroInstance, action);
    }
}
