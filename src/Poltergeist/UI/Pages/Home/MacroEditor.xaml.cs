using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Pages.Home;

public sealed partial class MacroEditor : UserControl
{
    public KeyValuePair<string, string>[] Templates { get; set; }
    public string? SelectedTemplateKey { get; set; }

    public MacroEditor(Dictionary<string, string> templates)
    {
        Templates = templates.ToArray();

        InitializeComponent();
    }
}
