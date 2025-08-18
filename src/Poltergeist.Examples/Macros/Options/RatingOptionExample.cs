using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class RatingOptionExample : UnrunnableMacro
{
    public RatingOptionExample() : base()
    {
        Title = nameof(RatingOption);

        Category = "Options";

        Description = $"This example defines several {nameof(RatingOption)} with different configurations.";

        OptionDefinitions.Add(new RatingOption("Default")
        {
            DisplayLabel = nameof(RatingOption),
            Description = "default",
        });

        OptionDefinitions.Add(new RatingOption("AllowsEmpty")
        {
            DisplayLabel = nameof(RatingOption),
            Description = "{ AllowsEmpty = true }",
            AllowsEmpty = true,
        });

        OptionDefinitions.Add(new RatingOption("MaxRating")
        {
            DisplayLabel = nameof(RatingOption),
            Description = "{ MaxRating = 10 }",
            MaxRating = 10,
        });

        OptionDefinitions.Add(new RatingOption("Caption")
        {
            DisplayLabel = nameof(RatingOption),
            Description = "{ Caption = \"Hello\" }",
            Caption = "Hello",
        });

        OptionDefinitions.Add(new RatingOption("CaptionList")
        {
            DisplayLabel = nameof(RatingOption),
            Description = "{ CaptionList = [\"-\",\"D\",\"C\",\"B\",\"A\",\"S\"] }",
            CaptionList = ["-", "D", "C", "B", "A", "S"],
            AllowsEmpty = true,
        });

        OptionDefinitions.Add(new RatingOption("CaptionMethod")
        {
            DisplayLabel = nameof(RatingOption),
            Description = "{ CaptionMethod = x => $\"{x * 100}\" }",
            CaptionMethod = x => $"{x * 100}",
            AllowsEmpty = true,
        });
    }

}
