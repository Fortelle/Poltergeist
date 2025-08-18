namespace Poltergeist.Automations.Macros;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModuleDependencyAttribute<T> : Attribute where T : MacroModule
{
}
