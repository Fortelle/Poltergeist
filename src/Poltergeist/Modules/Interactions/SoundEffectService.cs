using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Helpers;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Poltergeist.Modules.Interactions;

public class SoundEffectService
{
    private readonly Dictionary<string, MediaPlayer> Players = new();
    private readonly Dictionary<string, Uri> SoundEffectUris = new();

    public SoundEffectService()
    {
    }

    public void Add(string key, string uriString)
    {
        if (!RuntimeHelper.IsMSIX && uriString.StartsWith("ms-appx:///"))
        {
            uriString = "file:///" + AppDomain.CurrentDomain.BaseDirectory + uriString["ms-appx:///".Length..];
        }
        SoundEffectUris.Add(key, new(uriString));
    }

    public void Play(string key)
    {
        PlayInternal(key);
    }

    public void Play(SoundEffectModel model)
    {
        Play(model.Key);
    }

    private void PlayInternal(string key)
    {
        if (!SoundEffectUris.TryGetValue(key, out var uri))
        {
            return;
        }

        if (!Players.TryGetValue(key, out var player))
        {
            player = new MediaPlayer
            {
                Source = MediaSource.CreateFromUri(uri),
            };
            Players.Add(key, player);
        }
        else
        {
            player.Position = default;
        }

        player.Play();
    }
}
