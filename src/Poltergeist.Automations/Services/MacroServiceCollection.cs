using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Poltergeist.Automations.Services;

public class MacroServiceCollection : ServiceCollection
{
    public List<Type> AutoloadTypes = new();

    public IServiceCollection AddAutoload<TService>() where TService : class
    {
        this.AddSingleton<TService>();
        AutoloadTypes.Add(typeof(TService));
        return this;
    }

    public IServiceCollection AddImplementationAndInterface<TImplementation, TService>()
        where TService : class
        where TImplementation : class, TService
    {
        this.AddSingleton<TImplementation>();
        this.AddSingleton<TService>(x => x.GetService<TImplementation>()!);
        return this;
    }

}
