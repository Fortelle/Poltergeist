using System.Numerics;

namespace Poltergeist.Automations.Structures.Parameters;

public class NumberOption<T> : OptionDefinition<T>, INumberOption where T : INumber<T>
{
    public NumberOptionLayout Layout { get; set; }

    public required T? Minimum { get; set; }
    public required T? Maximum { get; set; }
    public T? StepFrequency { get; set; }
    public string? ValueFormat { get; set; }

    public NumberOption(string key) : base(key, default!)
    {
    }

    public NumberOption(string key, T defaultValue) : base(key, defaultValue)
    {
    }

    double? INumberOption.Minimum => Minimum is null ? null : Convert.ToDouble(Minimum);
    double? INumberOption.Maximum => Maximum is null ? null : Convert.ToDouble(Maximum);
    double? INumberOption.StepFrequency => StepFrequency is null ? null : Convert.ToDouble(StepFrequency);
}
