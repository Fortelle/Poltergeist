namespace Poltergeist.Modules.Events;

[AttributeUsage(AttributeTargets.Method)]
public class AppEventSubscriberAttribute : Attribute
{
    public bool Once { get; init; }
    public int Priority { get; init; }
}
