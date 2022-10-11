using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Background;

public class BackgroundService : MacroService
{
    private TextPanelModel BackgroundPanel { get; set; }

    public BackgroundService(MacroProcessor processor,
        PanelService panelService,
        HookService hooks
        ) : base(processor)
    {
        BackgroundPanel = panelService.Create<TextPanelModel>(panel =>
        {
            panel.Key = "poltergeist-background";
            panel.Header = "Background(dev)";
        });

        hooks.Register("process_started", Create);
    }

    private void Create(object[] args)
    {
        var classes = new List<Type>();
        classes.Add(Processor.Macro.GetType());
        while(true)
        {
            var baseType = classes.Last().BaseType;
            if (baseType == null) break;
            if (baseType.Name == "Object") break;
            classes.Add(baseType);
        }

        var family = classes.Select(type => {
            var s = type.Name;
            var interfaces = type.GetInterfaces().Except(type.BaseType.GetInterfaces()).Select (x=>x.Name).ToArray();
            if (interfaces.Length > 0)
            {
                s += " (" + string.Join(", ", interfaces) + ")";
            }
            return s;
        }).ToArray();

        var processer = (MacroProcessor)Processor; // ???

        var optionList = processer.Options.Select(x => $"{x.Key}({x.Value?.GetType().Name ?? "null"})  = {x.Value}").ToArray();

        var envList = processer.Environments.Select(x => $"{x.Key}({x.Value?.GetType().Name ?? "null"})  = {x.Value}").ToArray();

        var moduleList = Processor.Macro.Modules.Select(x => $"{x.GetType().Name}").ToArray();

        var serviceList = processer.ServiceList.Where(x=>!x.StartsWith("IOptions") && !x.StartsWith("IConfigureOptions")).ToArray();

        WriteArray("Classes", family);
        WriteArray("UserOptions", optionList);
        WriteArray("Environments", envList);
        WriteArray("Modules", moduleList);
        WriteArray("Services", serviceList);
    }

    private void WriteArray(string title, string[] lines)
    {
        var list = new List<TextLine>();

        list.Add(new()
        {
            Text = $"{title} ({lines.Length})",
            IsBold = true,
        });

        if (lines.Length > 0)
        {
            foreach (var line in lines)
            {
                list.Add(new()
                {
                    Text = "\t" + line
                });
            }
        }
        else
        {
            list.Add(new()
            {
                Text = "\t" + "(None)",
                Color = Colors.Gray,
            });
        }

        list.Add(new());

        BackgroundPanel.WriteLines(list.ToArray());
    }

}
