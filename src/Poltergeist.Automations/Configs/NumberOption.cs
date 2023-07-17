using System;
using System.Numerics;

namespace Poltergeist.Automations.Configs;

public class NumberOption<T> : OptionItem<T>, INumberOptionItem where T :  INumber<T>
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

    double? INumberOptionItem.Minimum => Minimum is null ? null : Convert.ToDouble(Minimum);
    double? INumberOptionItem.Maximum => Maximum is null ? null : Convert.ToDouble(Maximum);
    double? INumberOptionItem.StepFrequency => StepFrequency is null ? null : Convert.ToDouble(StepFrequency);
}
