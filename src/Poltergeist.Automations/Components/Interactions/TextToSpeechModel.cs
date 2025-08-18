namespace Poltergeist.Automations.Components.Interactions;

public class TextToSpeechModel : InteractionModel
{
    public required string Text { get; set; }
    public string? VoiceToken { get; set; }
}
