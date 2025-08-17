using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class GlobalOptionsService : ServiceBase
{
    public SavablePredefinedCollection GlobalOptions { get; set; } = new();

    public GlobalOptionsService(AppEventService eventService)
    {
        Load();

        eventService.Subscribe<AppWindowClosedEvent>(OnAppWindowClosed);
    }

    private void Load()
    {
        var filepath = PoltergeistApplication.Paths.GlobalMacroOptionsFile;

        try
        {
            GlobalOptions.Load(filepath);

            Logger.Trace($"Loaded the global options.", new
            {
                Path = filepath,
            });
        }
        catch (FileNotFoundException)
        {
            Logger.Trace($"Skipped loading the global options: File does not exist.", new
            {
                Path = filepath,
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load the global options: {ex.Message}", new
            {
                Path = filepath,
                Exception = ex.GetType().Name,
                ex.Message,
            });
            if (PoltergeistApplication.Current.IsDevelopment)
            {
                PoltergeistApplication.ShowTeachingTip($"Failed to load global options");
            }
        }
    }

    public void Save()
    {
        try
        {
            if (GlobalOptions.Save())
            {
                Logger.Trace("Saved the global options.", new { GlobalOptions.FilePath });

                if (PoltergeistApplication.Current.IsDevelopment)
                {
                    PoltergeistApplication.ShowTeachingTip($"Saved global options");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save the global options: {ex.Message}");

            if (PoltergeistApplication.Current.IsDevelopment)
            {
                PoltergeistApplication.ShowTeachingTip($"Failed to save global options");
            }
        }
    }

    private void OnAppWindowClosed(AppWindowClosedEvent _)
    {
        Save();
    }
}
