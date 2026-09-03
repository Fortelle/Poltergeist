namespace Poltergeist.Automations.Services;

public interface IServiceDependency
{
    ServiceLifetime Lifetime { get; }
}
