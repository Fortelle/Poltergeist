using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Test;

public partial class ExampleGroup : MacroGroup
{

    [AutoLoad]
    public UnrunnableMacro RatingOptionExampleMacro = new("example_option_ratingoptions")
    {
        Title = "RatingOption Example",

        IsSingleton = true,

        UserOptions = {

           new RatingOption("default")
           {
               DisplayLabel = "Default",
           },

           new RatingOption("allowsempty")
           {
               DisplayLabel = $"{nameof(RatingOption.AllowsEmpty)} = true",
               AllowsEmpty = true,
           },

           new RatingOption("maxrating")
           {
               DisplayLabel = $"{nameof(RatingOption.MaxRating)} = 10",
               MaxRating = 10,
           },

           new RatingOption("defaultcaption")
           {
               DisplayLabel = $"{nameof(RatingOption.Caption)} = Hello",
               Caption = "Hello",
           },

           new RatingOption("captionlist")
           {
               DisplayLabel = $"{nameof(RatingOption.CaptionList)} = {{\"-\", \"D\", \"C\", \"B\", \"A\", \"S\"}}",
               CaptionList = ["-", "D", "C", "B", "A", "S"],
               AllowsEmpty = true,
           },

           new RatingOption("captionmethod")
           {
               DisplayLabel = $"{nameof(RatingOption.CaptionMethod)} = x => $\"{{x * 100}}\"",
               CaptionMethod = x => $"{x * 100}",
               AllowsEmpty = true,
           },

           new RatingOption("defaultvalue", 3)
           {
               DisplayLabel = $"base.{nameof(IParameterDefinition.DefaultValue)} = 3",
           },
        },
    };

}
