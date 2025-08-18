namespace Poltergeist.Automations.Structures.Parameters;

public interface INumberOption : IParameterDefinition
{
    NumberOptionLayout Layout { get; }

    double? Minimum { get; }
    double? Maximum { get; }
    double? StepFrequency { get; }
    string? ValueFormat { get; }
}
