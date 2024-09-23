using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Hooks;

public abstract class MacroHook
{
    private IUserProcessor? _processor;
    public IUserProcessor Processor
    {
        get
        {
            if (_processor == null)
            {
                throw new InvalidOperationException("This property should not be used before the hook is raised.");
            }
            return _processor;
        }

        internal set => _processor = value;
    }
}
