using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class BackgroundService : MacroService, IAutoloadable
{
    private TextInstrument BackgroundInstrument { get; set; }

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

        hooks.Register<ProcessStartedHook>(Create);
    }

    private void Create(ProcessStartedHook args)
    {
        var classes = new List<Type>
        {
            Processor.Macro.GetType()
        };
        while (true)
        {
            var baseType = classes.Last().BaseType;
            if (baseType == null)
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

        var processer = (MacroProcessor)Processor; // ???

        var optionList = processer.Options
            .Select(x => $"{x.Key}({x.Value?.GetType().Name ?? "null"})  = {x.Value}")
            .ToArray();

        var envList = processer.Environments
            .Select(x => $"{x.Key}({x.Value?.GetType().Name ?? "null"})  = {x.Value}")
            .ToArray();

        var moduleList = Processor.Macro.Modules
            .Select(x => $"{x.GetType().Name}")
            .ToArray();

        var serviceList = processer.ServiceCollection!
            .Select(x => x.ServiceType.Name)
            .Where(x => !x.StartsWith("IOptions") && !x.StartsWith("IConfigureOptions"))
            .ToArray();

        var serviceOptions = processer.ServiceCollection!
            .Select(x => x.ServiceType.Name)
            .Where(x => x.StartsWith("IOptions") || x.StartsWith("IConfigureOptions"))
            .ToArray();

        WriteArray("Classes", family);
        WriteArray("UserOptions", optionList);
        WriteArray("Environments", envList);
        WriteArray("Modules", moduleList);
        WriteArray("Services", serviceList);
        WriteArray("ServiceOptions", serviceOptions);
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
