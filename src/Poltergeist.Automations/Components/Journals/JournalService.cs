using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Journals;

public class JournalService : MacroService
{
    private JournalInstrument? JournalInstrument;

    public bool AutoSave { get; set; }

    public bool NewPanel { get; set; }

    public JournalService(MacroProcessor processor, HookService hookService) : base(processor)
    {
        hookService.Register<ProcessorEndingHook>(ProcessorEndingHook);
    }

    public void AppendLine(string text)
    {
        if (JournalInstrument is null)
        {
            CreateInstrument();
        }

        JournalInstrument!.Add(text);
    }

    private void ProcessorEndingHook(ProcessorEndingHook hook)
    {
        TrySave(hook.EndTime);
    }

    private void TrySave(DateTime time)
    {
        if (AutoSave)
        {
            return;
        }

        if (JournalInstrument is null || JournalInstrument.Lines.Count == 0)
        {
            return;
        }

        if (!Processor.Environments.TryGetValue<string>("private_folder", out var privateFolder))
        {
            return;
        }

        var path = Path.Combine(privateFolder, "Journals", $"{time:yyyy-dd-MM HH-mm-ss}.md");
        Save(path);
    }

    public void Save(string path)
    {
        var folder = Path.GetDirectoryName(path);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllLines(path, JournalInstrument?.Lines ?? []);
    }

    private void CreateInstrument()
    {
        JournalInstrument = Processor.GetService<JournalInstrument>();

        if (NewPanel)
        {
            var panel = Processor.GetService<PanelService>().Create(new("poltergeist-journal", "Journal")
            {
                ToLeft = true,
                Active = true,
            });
            panel.Instruments.Add(JournalInstrument);
        }
        else
        {
            JournalInstrument.Title = "Journal:";
            Processor.GetService<DashboardService>().Add(JournalInstrument);
        }
    }
}
