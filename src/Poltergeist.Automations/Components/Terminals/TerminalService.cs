using System.Windows.Media;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Windows;

namespace Poltergeist.Automations.Components.Terminals;

public class TerminalService : MacroService
{
    private TextPanelModel TerminalPanel { get; set; }
    private CmdHost Host { get; set; }

    public string WorkingDirectory { get; set; }
    public string PanelHeader { get; set; } = "Terminal";
    public string PanelName { get; set; } = "poltergeist-terminal";

    public TerminalService(MacroProcessor processor) : base(processor)
    {
    }

    public void Start()
    {
        var panelService = Processor.GetService<PanelService>();
        TerminalPanel = panelService.Create<TextPanelModel>(panel =>
        {
            panel.Key = PanelName;
            panel.Header = PanelHeader;
            panel.BackgroundColor = Color.FromRgb(16, 16, 16);
            panel.ForegroundColor = Color.FromRgb(252, 252, 252);

            panel.Colors.Add("input", Colors.LimeGreen);
            panel.Colors.Add("output", Colors.White);
        });
        Host = new(WorkingDirectory);
        Host.Start();
    }

    public void Execute(string command)
    {
        TerminalPanel.WriteLine(new TextLine()
        {
            Category = "input",
            Text = "> " + command,
        });

        Host.TryExecute(command, out var output);

        if (!string.IsNullOrEmpty(output))
        {
            TerminalPanel.WriteLine(new TextLine()
            {
                Category = "output",
                Text = output,
            });
        }
    }

    public void Close()
    {
        Host.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();

        Close();
    }
}
