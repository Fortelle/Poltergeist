using Microsoft.UI;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;
using Windows.UI;

namespace Poltergeist.Automations.Components.Terminals;

// todo: move to default transient
public class TerminalService : MacroService
{
    public string PanelHeader { get; set; } = ResourceHelper.Localize("Poltergeist.Automations/Resources/Terminal_Header");
    public string PanelName { get; set; } = "poltergeist-terminal";
    public string? WorkingDirectory { get; set; }

    private CmdHost? Host;
    private readonly TextInstrument TerminalInstrument;

    public TerminalService(MacroProcessor processor, TextInstrument terminalInstrument) : base(processor)
    {
        TerminalInstrument = terminalInstrument;
    }

    public void Start()
    {
        Logger.Debug($"Starting <{nameof(TerminalService)}>.");

        TerminalInstrument.Stretch = true;
        TerminalInstrument.AutoScroll = true;
        TerminalInstrument.BackgroundColor = Color.FromArgb(255, 16, 16, 16);
        TerminalInstrument.ForegroundColor = Color.FromArgb(255, 252, 252, 252);
        TerminalInstrument.Templates.Add("input", new() { Foreground = Colors.LimeGreen });
        TerminalInstrument.Templates.Add("output", new() { Foreground = Colors.White });

        Processor.GetService<PanelService>().Create(new(PanelName, PanelHeader, TerminalInstrument)
        {
            IsFilled = true,
        });

        Host = new(WorkingDirectory ?? "");
        Host.Start();

        Logger.Debug($"Started <{nameof(TerminalService)}>.");
    }

    public string Execute(string command)
    {
        Logger.Debug($"Executing command line \"{command}\".");

        TerminalInstrument.WriteLine(new TextLine("> " + command)
        {
            TemplateKey = "input",
        });

        Host!.TryExecute(command, out var output);

        if (!string.IsNullOrEmpty(output))
        {
            TerminalInstrument.WriteLine(new TextLine(output)
            {
                TemplateKey = "output",
            });
        }

        Logger.Debug($"Executed command line.", new { output });

        return output ?? "";
    }

    public void Close()
    {
        Host?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }

        base.Dispose(disposing);
    }
}
