using Newtonsoft.Json.Linq;

namespace Poltergeist.Automations.Configs;

public class UndefinedOptionItem : OptionItem<JToken?>
{
    public UndefinedOptionItem(string key, JToken? value) : base(key, value)
    {
    }
}
