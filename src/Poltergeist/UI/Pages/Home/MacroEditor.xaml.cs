using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Pages.Home;

public sealed partial class MacroEditor : UserControl
{
    public bool IsNew { get; set; }
    public string? MacroName { get; set; }
    public KeyValuePair<string, string>[] Templates { get; set; }
    public string? SelectedTemplateKey { get; set; }

    public MacroEditor(Dictionary<string, string> templates, bool isNew)
    {
        IsNew = isNew;
        Templates = templates.ToArray();

        InitializeComponent();
    }
}
