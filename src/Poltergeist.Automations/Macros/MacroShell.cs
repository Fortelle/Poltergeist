using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public class MacroShell
{
    public const string DefaultIconUri = @"ms-appx:///Poltergeist.Automations/Assets/macro_16px.png";

    public string ShellKey { get; }

    public IFrontMacro? Template { get; set; }
    public MacroProperties Properties { get; set; }

    public ParameterDefinitionValueCollection? UserOptions { get; private set; }
    public ParameterDefinitionValueCollection? Statistics { get; private set; }
    public ProcessHistoryCollection? History { get; private set; }

    public string? PrivateFolder { get; set; }

    public bool IsLoaded { get; set; }

    public string TemplateKey => Properties.TemplateKey;
    public string Title => Properties.Title ?? Template?.Title ?? ShellKey;
    public string? Description => Properties.Description ?? Template?.Description;
    public string? Icon => Properties.Icon ?? Template?.Icon;

    public MacroShell(IFrontMacro template)
    {
        Template = template;
        ShellKey = template.IsSingleton ? template.Key : $"{template.Key}@{Guid.NewGuid()}";
        Properties = new()
        {
            TemplateKey = template.Key,
            ShellKey = ShellKey,
        };

        if (!template.IsSingleton)
        {
            Properties.CreatedTime = DateTime.Now;
        }
    }

    public MacroShell(IFrontMacro? template, MacroProperties properties)
    {
        Template = template;
        Properties = properties;
        ShellKey = properties.ShellKey;
    }

    public MacroShell(MacroProperties properties)
    {
        Properties = properties;
        ShellKey = properties.ShellKey;
    }

    public void Load()
    {
        if (Template is null)
        {
            return;
        }

        if (IsLoaded)
        {
            return;
        }

        Template.Initialize();

        if (Template.Status != MacroStatus.Initialized)
        {
            return;
        }

        if (!string.IsNullOrEmpty(PrivateFolder))
        {
            try
            {
                UserOptions = new(Template.UserOptions);
                UserOptions.Load(Path.Combine(PrivateFolder, "useroptions.json"));
            }
            catch
            {
            }

            try
            {
                Statistics = new(Template.Statistics);
                Statistics.Load(Path.Combine(PrivateFolder, "statistics.json"));
            }
            catch
            {
            }

            try
            {
                History = new();
                History.Load(Path.Combine(PrivateFolder, "history.json"));
            }
            catch
            {
            }
        }
    }

}
