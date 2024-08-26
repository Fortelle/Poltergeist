namespace Poltergeist.Automations.Structures.Parameters;

public interface INumberOption : IParameterDefinition
{
    public NumberOptionLayout Layout { get; }

    public double? Minimum { get; }
    public double? Maximum { get; }
    public double? StepFrequency { get; }
    public string? ValueFormat { get; }
}
