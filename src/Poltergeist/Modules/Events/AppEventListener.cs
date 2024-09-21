namespace Poltergeist.Modules.Events;

public class AppEventListener
{
    public Type Type { get; }

    public Delegate Callback { get; }

    public bool Once { get; set; }

    public int Priority { get; set; }

    public string? Subscriber { get; set; }

    public bool IsAsync { get; set; }

    public AppEventListener(Type type, Delegate callback)
    {
        Type = type;
        Callback = callback;
    }
}
