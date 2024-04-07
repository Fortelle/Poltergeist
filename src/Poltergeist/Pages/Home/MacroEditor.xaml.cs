using Microsoft.UI.Xaml.Controls;
using Poltergeist.Services;

namespace Poltergeist.Pages.Home;

public sealed partial class MacroEditor : UserControl
{
    public bool IsNew { get; set; }
    public string? MacroName { get; set; }
    public Dictionary<string, string> Templates { get; set; }
    public string? SelectedTemplateKey { get; set; }

    public MacroEditor(bool isNew)
    {
        IsNew = isNew;

        var macroManager = App.GetService<MacroManager>();
        Templates = macroManager.Templates.ToDictionary(x => x.Key, x => x.Title);

        this.InitializeComponent();
    }
}
