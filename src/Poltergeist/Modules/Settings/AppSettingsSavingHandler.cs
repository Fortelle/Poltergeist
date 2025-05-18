using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Settings;

public class AppSettingsSavingHandler(ParameterDefinitionValueCollection settings) : AppEventHandler
{
    public ParameterDefinitionValueCollection Settings => settings;
}
