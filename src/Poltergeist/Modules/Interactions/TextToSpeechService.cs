using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Settings;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Poltergeist.Modules.Interactions;

public class TextToSpeechService
{
    private MediaPlayer? MediaPlayer;
    private SpeechSynthesizer? Synthesizer;

    public TextToSpeechService(AppSettingsService appSettingsService, AppEventService eventService)
    {
        var voices = SpeechSynthesizer.AllVoices.ToDictionary(x => x.Id.Split('\\')[^1], x => x.DisplayName);
        var defaultVoice = SpeechSynthesizer.DefaultVoice.Id.Split('\\')[^1];
        appSettingsService.Settings.AddDefinition(new ChoiceOption<string>("tts_voice", voices, defaultVoice));
    }

    private void CheckVoice(string? voiceToken)
    {
        Synthesizer ??= new SpeechSynthesizer();

        voiceToken ??= PoltergeistApplication.GetService<AppSettingsService>().Settings.Get<string>("tts_voice")!;
        if (!Synthesizer.Voice.Id.EndsWith('\\' + voiceToken))
        {
            Synthesizer.Voice = SpeechSynthesizer.AllVoices.FirstOrDefault(x => x.Id.EndsWith('\\' + voiceToken)) ?? SpeechSynthesizer.DefaultVoice;
        }
    }

    public void Speech(string text, string? voiceToken = null)
    {
        if (!PoltergeistApplication.Current.IsReady)
        {
            return;
        }

        Synthesizer ??= new SpeechSynthesizer();

        CheckVoice(voiceToken);

        MediaPlayer?.Dispose();

        Synthesizer.SynthesizeTextToStreamAsync(text).AsTask().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                MediaPlayer = new MediaPlayer
                {
                    Source = MediaSource.CreateFromStream(task.Result, "audio/wav")
                };
                MediaPlayer.Play();
            }
        });
    }

    public void Speech(TextToSpeechModel model)
    {
        Speech(model.Text, model.VoiceToken);
    }
}
