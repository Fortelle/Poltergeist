namespace Poltergeist.Automations.Parameters;

public record class ParameterDefinitionValuePair(IParameterDefinition Definition, object? Value)
{
    public string DisplayValue => Definition.FormatValue(Value);
}
