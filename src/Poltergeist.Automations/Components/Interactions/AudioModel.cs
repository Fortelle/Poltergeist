namespace Poltergeist.Automations.Components.Interactions;

public class AudioModel : NotificationModel
{
    public required string FilePath { get; set; }

    public bool IsLooping { get; set; }

    public TimeSpan Duration { get; set; }
}
