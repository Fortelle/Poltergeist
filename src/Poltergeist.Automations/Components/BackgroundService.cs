using Microsoft.UI;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class BackgroundService : MacroService, IAutoloadable
{
    private readonly TextInstrument BackgroundInstrument;

    public BackgroundService(MacroProcessor processor,
        PanelService panelService,
        HookService hooks
        ) : base(processor)
    {
        BackgroundInstrument = Processor.GetService<TextInstrument>();
        panelService.Create(new("poltergeist-background", "Background(dev)", BackgroundInstrument)
        {
            IsFilled = true,
        });

        hooks.Register<ProcessorStartedHook>(Create);
    }

    private void Create(ProcessorStartedHook args)
    {
        var classes = new List<Type>
        {
            Processor.Macro.GetType()
        };
        while (true)
        {
            var baseType = classes.Last().BaseType;
            if (baseType is null)
            {
                break;
            }

            if (baseType.Name == "Object")
            {
                break;
            }

            classes.Add(baseType);
        }

        var family = classes.Select(type =>
        {
            var s = type.Name;
            var interfaces = type.GetInterfaces().Except(type.BaseType!.GetInterfaces()).Select(x => x.Name).ToArray();
            if (interfaces.Length > 0)
            {
                s += " (" + string.Join(", ", interfaces) + ")";
            }
            return s;
        }).ToArray();

        var processor = (MacroProcessor)Processor;
        var macro = (MacroBase)Processor.Macro;

        var optionList = processor.Options
            .Select(x => $"{x.Key}({x.Value?.GetType().Name ?? "null"}) = {x.Value}")
            .ToArray();

        var envList = processor.Environments
            .Select(x => $"{x.Key}({x.Value?.GetType().Name ?? "null"}) = {x.Value}")
            .ToArray();

        var moduleList = macro.Modules
            .Select(x => $"{x.GetType().Name}")
            .ToArray();

        var serviceList = processor.ServiceCollection!
            .Select(x => x.ServiceType.Name)
            .Where(x => !x.StartsWith("IOptions") && !x.StartsWith("IConfigureOptions"))
            .ToArray();

        var serviceOptions = processor.ServiceCollection!
            .Select(x => x.ServiceType.Name)
            .Where(x => x.StartsWith("IOptions") || x.StartsWith("IConfigureOptions"))
            .ToArray();

        WriteArray("Inheritance", family);
        WriteArray("User Options", optionList);
        WriteArray("Environments", envList);
        WriteArray("Modules", moduleList);
        WriteArray("Services", serviceList);
        WriteArray("Service Options", serviceOptions);
    }

    private void WriteArray(string title, string[] lines)
    {
        var list = new List<TextLine>
        {
            new($"{title} ({lines.Length})")
            {
                IsBold = true,
            }
        };

        if (lines.Length > 0)
        {
            foreach (var line in lines)
            {
                list.Add(new("\t" + line));
            }
        }
        else
        {
            list.Add(new("\t" + "(None)")
            {
                Foreground = Colors.Gray,
            });
        }

        list.Add(new());

        BackgroundInstrument.WriteLines(list.ToArray());
    }

}
