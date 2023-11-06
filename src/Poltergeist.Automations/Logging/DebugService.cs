using System.Drawing;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Logging;

public class DebugService : MacroService
{
    private ImageInstrument? OutputInstrument { get; set; }

    public DebugService(MacroProcessor processor) : base(processor)
    {
    }

    public void Initialize()
    {
        if (OutputInstrument is not null)
        {
            return;
        }

        OutputInstrument = Processor.GetService<DashboardService>().Create<ImageInstrument>(li =>
        {
            li.Title = "Images:";
        });
    }

    public void Trace(object variable, [CallerArgumentExpression(nameof(variable))] string? name = null)
    {
        Logger.Trace(variable, name);
    }

    public void Write(Bitmap image, string text = "")
    {
        if (OutputInstrument is null)
        {
            Initialize();
        }

        OutputInstrument!.Add(new(new(image))
        {
            Label = text,
        });
    }
}
