namespace Poltergeist.Automations.Structures.Parameters;

public record class ParameterDefinitionValuePair(IParameterDefinition Definition, object? Value)
{
    public string DisplayValue => Definition.FormatValue(Value);
}
