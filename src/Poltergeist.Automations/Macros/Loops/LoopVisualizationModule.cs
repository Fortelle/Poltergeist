using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopVisualizationModule : MacroModule
{
    public static readonly EntryDefinition<string> TitleDefinition = new("loop_visualization_title");
    public static readonly EntryDefinition<LoopInstrumentView> ViewDefinition = new("loop_visualization_view");

    [MacroHook]
    public static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        if (!processor.SessionStorage.ContainsKey(TitleDefinition))
        {
            var title = LocalizationUtil.Localize("Loops_Instrument_Title");
            processor.SessionStorage.Add(TitleDefinition, title);
        }

        var instrumentType = processor.SessionStorage.GetValueOrDefault(ViewDefinition, LoopInstrumentView.List);

        switch (instrumentType)
        {
            case LoopInstrumentView.Tile:
                InstallProgressTile(processor);
                break;
            case LoopInstrumentView.ProgressBar:
                InstallProgressBar(processor);
                break;
            case LoopInstrumentView.List:
                InstallProgressList(processor);
                break;
        }
    }

    private static void InstallProgressTile(IMacroProcessorShared processor)
    {
        var dashboardService = processor.GetService<DashboardService>();
        var hookService = processor.GetService<HookService>();
        var title = processor.SessionStorage.Get(TitleDefinition);

        var gridInstrument = dashboardService.Create<ProgressTileInstrument>(instrument =>
        {
            instrument.Title = title;
            instrument.IsSticky = true;
        });

        hookService.Register<LoopStartingHook>((s, e) =>
        {
            var maxCount = e.MaxIterations;
            if (maxCount > 0)
            {
                for (var i = 0; i < maxCount; i++)
                {
                    var info = GetInfo(hookService) ?? new ProgressInstrumentInfo()
                    {
                        Status = ProgressStatus.Idle,
                    };
                    gridInstrument.Add(new(info));
                }
            }
        });

        hookService.Register<UpdateInstrumentInfoHook>((s, e) =>
        {
            if (e.Info == null)
            {
                return;
            }
            gridInstrument.Update(e.Index, new(e.Info));
        });

        hookService.Register<IterationStartedHook>((s, e) =>
        {
            gridInstrument.Update(e.Index, new(ProgressStatus.Busy));
        });

        hookService.Register<IterationEndedHook>((s, e) =>
        {
            gridInstrument.Update(e.Index, new(ProgressStatus.Success));
        });

        hookService.Register<IterationErrorHook>((s, e) =>
        {
            gridInstrument.Update(e.Index, new(ProgressStatus.Failure));
        });

        hookService.Register<IterationCanceledHook>((s, e) =>
        {
            gridInstrument.Update(e.Index, new(ProgressStatus.Warning));
        });
    }

    private static void InstallProgressBar(IMacroProcessorShared processor)
    {
        var dashboardService = processor.GetService<DashboardService>();
        var hookService = processor.GetService<HookService>();
        var title = processor.SessionStorage.Get(TitleDefinition);
        var maxCount = -1;

        var listInstrument = dashboardService.Create<ProgressListInstrument>(instrument =>
        {
            instrument.Title = title;
            instrument.IsSticky = true;
        });

        hookService.Register<LoopStartingHook>((s, e) =>
        {
            maxCount = e.MaxIterations;
            listInstrument.Add(new(ProgressStatus.Busy)
            {
                Text = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_Text"),
                Progress = maxCount <= 0 ? 1 : 0,
                Subtext = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_Subtext"),
            });
        });

        hookService.Register<IterationStartedHook>((s, e) =>
        {
            if (maxCount <= 0)
            {
                listInstrument.Update(0, new()
                {
                    Subtext = $"{e.Index + 1}",
                });
            }
            else
            {
                listInstrument.Update(0, new()
                {
                    Progress = (e.Index + 1d) / (maxCount + 1),
                    Subtext = $"{e.Index + 1} / {maxCount}",
                });
            }
        });

        hookService.Register<UpdateInstrumentInfoHook>((s, e) =>
        {
            if (e.Info == null)
            {
                return;
            }
            listInstrument.Update(0, new(e.Info));
        });

        hookService.Register<ProcessorCompletedHook>((s, e) =>
        {
            var status = e.Conclusion switch
            {
                ProcessorConclusion.Success => ProgressStatus.Success,
                ProcessorConclusion.Canceled or ProcessorConclusion.Terminated => ProgressStatus.Warning,
                _ => ProgressStatus.Failure,
            };
            listInstrument.Update(0, new(status)
            {
                Subtext = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_SuccessText"),
                Progress = 1,
            });
        });

    }

    private static void InstallProgressList(IMacroProcessorShared processor)
    {
        var dashboardService = processor.GetService<DashboardService>();
        var hookService = processor.GetService<HookService>();
        var title = processor.SessionStorage.Get(TitleDefinition);

        var listInstrument = dashboardService.Create<ProgressListInstrument>(instrument =>
        {
            instrument.Title = title;
        });

        hookService.Register<LoopStartingHook>((s, e) =>
        {
            var maxCount = e.MaxIterations;
            if (maxCount > 0)
            {
                for (var i = 0; i < maxCount; i++)
                {
                    var info = GetInfo(hookService) ?? new ProgressInstrumentInfo()
                    {
                        Status = ProgressStatus.Idle,
                        Text = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_ProgressText", i + 1)
                    };
                    listInstrument.Add(new(info));
                }
            }
        });

        hookService.Register<IterationStartedHook>((s, e) =>
        {
            listInstrument.Update(e.Index, new(ProgressStatus.Busy)
            {
                Text = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_ProgressText", e.Index + 1),
            });
        });

        hookService.Register<UpdateInstrumentInfoHook>((s, e) =>
        {
            if (e.Info == null)
            {
                return;
            }
            listInstrument.Update(e.Index, new(e.Info!));
        });

        hookService.Register<IterationEndedHook>((s, e) =>
        {
            listInstrument.Update(e.Index, new(ProgressStatus.Success));
        });

        hookService.Register<IterationErrorHook>((s, e) =>
        {
            listInstrument.Update(e.Index, new(ProgressStatus.Failure));
        });

        hookService.Register<IterationCanceledHook>((s, e) =>
        {
            listInstrument.Update(e.Index, new(ProgressStatus.Warning));
        });
    }

    private static ProgressInstrumentInfo? GetInfo(HookService hookService)
    {
        var hook = new GetInitialInfoHook();
        hookService.Raise(hook);
        return hook.Info;
    }
}
