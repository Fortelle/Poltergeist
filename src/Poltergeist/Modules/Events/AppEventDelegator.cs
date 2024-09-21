namespace Poltergeist.Modules.Events;

public class AppEventDelegator
{
    public string EventName { get; set; }

    public List<AppEventListener> Listeners { get; } = new();

    public AppEventDelegator(Type handlerType)
    {
        EventName = handlerType.Name.Replace("Handler", null);
    }

    public AppEventListener? GetListener(Delegate @delegate)
    {
        return Listeners.FirstOrDefault(x => x.Callback == @delegate);
    }
}
