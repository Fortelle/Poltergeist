namespace Poltergeist.Automations.Structures.Parameters;

public class RatingOption : OptionDefinition<int>
{
    public int MaxRating { get; set; }

    public bool AllowsEmpty { get; set; }

    public string? Caption { get; set; }

    public string[]? CaptionList { get; set; }

    public Func<int, string>? CaptionMethod { get; set; }

    public RatingOption(string key) : this(key, default)
    {
    }

    public RatingOption(string key, int defaultValue) : base(key, defaultValue)
    {
        MaxRating = 5;
    }
}
