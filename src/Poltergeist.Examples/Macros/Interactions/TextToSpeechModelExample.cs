using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;
using Windows.Media.SpeechSynthesis;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class TextToSpeechModelExample : BasicMacro
{
    public TextToSpeechModelExample() : base()
    {
        Title = nameof(TextToSpeechModel);

        Category = "Interactions";

        Description = $"This example shows how to synthesize texts by using {nameof(TextToSpeechModel)}.";

        OptionDefinitions.Add(new TextOption("tts_text", "Hello world!"));

        var voices = new Dictionary<string, string>()
        {
            { "", "" }
        };
        foreach (var vi in SpeechSynthesizer.AllVoices)
        {
            voices.Add(vi.Id.Split('\\')[^1], vi.DisplayName);
        }
        OptionDefinitions.Add(new ChoiceOption<string>("tts_voice", voices, ""));

        Execute = (args) =>
        {
            var text = args.Processor.Options.Get<string>("tts_text");
            var voice = args.Processor.Options.Get<string>("tts_voice");

            var model = new TextToSpeechModel()
            {
                Text = text,
                VoiceToken = string.IsNullOrEmpty(voice) ? null : voice,
            };
            args.Processor.GetService<InteractionService>().Push(model);
        };
    }

}
