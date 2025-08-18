using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IUserMacro : IMacroBase
{
    ParameterValueCollection ExtraData { get; }
}