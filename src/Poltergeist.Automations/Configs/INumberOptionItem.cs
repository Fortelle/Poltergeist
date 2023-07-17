namespace Poltergeist.Automations.Configs;

public interface INumberOptionItem : IOptionItem
{
    public NumberOptionLayout Layout { get; }

    public double? Minimum { get; }
    public double? Maximum { get; }
    public double? StepFrequency { get; }
    public string? ValueFormat { get; }
}
