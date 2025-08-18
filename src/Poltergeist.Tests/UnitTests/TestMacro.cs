using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

public class TestMacro : MacroBase
{
    public Action<IConfigurableProcessor>? Configure { get; set; }
    public Action<IPreparableProcessor>? Prepare { get; set; }
    public Action<IUserProcessor>? Execute { get; set; }
    public Func<IUserProcessor, Task>? ExecuteAsync { get; set; }

    public TestMacro() : base()
    {
    }

    public TestMacro(string name) : base(name)
    {
    }

    protected override void OnConfigure(IConfigurableProcessor processor)
    {
        base.OnConfigure(processor);

        Configure?.Invoke(processor);
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        processor.AddStep(new("execution", () =>
        {
            if (Execute is not null)
            {
                Execute((IUserProcessor)processor);
            }
            else if (ExecuteAsync is not null)
            {
                ExecuteAsync((IUserProcessor)processor).GetAwaiter().GetResult();
            }
        })
        {
            IsDefault = true,
        });

        Prepare?.Invoke(processor);
    }
}
