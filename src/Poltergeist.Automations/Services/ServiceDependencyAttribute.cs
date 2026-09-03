namespace Poltergeist.Automations.Services;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServiceDependencyAttribute<T>(ServiceLifetime lifetime) : Attribute, IServiceDependency where T : class
{
    public ServiceLifetime Lifetime => lifetime;
}
