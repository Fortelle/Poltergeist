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

    public void ExecuteAction(MacroAction action, MacroActionArguments arguments)
    {
        if (Template is null)
        {
            throw new InvalidOperationException("The macro template does not exist.");
        }

        if (Template.Status != MacroStatus.Initialized)
        {
            throw new InvalidOperationException("The macro template has not been initialized correctly.");
        }

        if (!Template.Actions.Contains(action))
        {
            throw new ArgumentException("The action is not owned by the macro template.");
        }

        if (action.Execute is not null)
        {
            try
            {
                action.Execute(arguments);

                if (!string.IsNullOrEmpty(arguments.Message))
                {
                    _ = InteractionService.UIShowAsync(new TipModel()
                    {
                        Text = arguments.Message,
                    });
                }
            }
            catch (Exception exception)
            {
                _ = InteractionService.UIShowAsync(new TipModel()
                {
                    Text = exception.Message,
                });
            }
        }
        else if (action.ExecuteAsync is not null)
        {
            Task.Run(async () => {
                CancellationTokenSource? cts = null;

                if (action.IsCancellable)
                {
                    cts = new();
                    arguments.CancellationToken = cts.Token;
                }

                _ = InteractionService.UIShowAsync(new ProgressModel()
                {
                    IsOn = true,
                    Title = action.ProgressTitle ?? action.Text,
                    CancellationTokenSource = cts,
                });

                try
                {
                    await action.ExecuteAsync(arguments);

                    if (!string.IsNullOrEmpty(arguments.Message))
                    {
                        _ = InteractionService.UIShowAsync(new TipModel()
                        {
                            Text = arguments.Message,
                        });
                    }
                }
                catch (Exception exception)
                {
                    _ = InteractionService.UIShowAsync(new TipModel()
                    {
                        Text = exception.Message,
                    });
                }

                _ = InteractionService.UIShowAsync(new ProgressModel()
                {
                    IsOn = false,
                });

                cts?.Dispose();
            });
        }
        else
        {
            _ = InteractionService.UIShowAsync(new TipModel()
            {
                Text = "The action is empty.",
            });
        }
    }
}
