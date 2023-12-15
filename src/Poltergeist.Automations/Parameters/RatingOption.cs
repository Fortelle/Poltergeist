namespace Poltergeist.Automations.Parameters;

public class RatingOption : OptionItem<int>
{
    public int MaxRating { get; set; }

    public bool AllowsEmpty { get; set; }

    public string? Caption { get; set; }

    public string[]? CaptionList { get; set; }

    public Func<int, string>? CaptionMethod { get; set; }

    public RatingOption(string key, int defaultValue = 0) : base(key, defaultValue)
    {
        MaxRating = 5;
    }
}
