using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IUserMacro : IMacroBase
{
    public ParameterValueCollection ExtraData { get; }
}