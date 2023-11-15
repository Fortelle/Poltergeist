using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Poltergeist.Automations.Components;

public class SoundService : MacroService
{
    private MediaPlayer? mediaPlayer;

    public SoundService(MacroProcessor processor) : base(processor)
    {
    }

    public void Play(string path, bool loop = false)
    {
        mediaPlayer?.Dispose();
        mediaPlayer = new MediaPlayer
        {
            Source = MediaSource.CreateFromUri(new Uri(path)),
        };
        if (loop)
        {
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }
        mediaPlayer.Play();
    }

    public void Play()
    {
        if (mediaPlayer is null)
        {
            return;
        }

        mediaPlayer.Play();
    }

    public void Pause()
    {
        if (mediaPlayer is null)
        {
            return;
        }

        mediaPlayer.Pause();
    }

    public void Stop()
    {
        if (mediaPlayer is null)
        {
            return;
        }

        mediaPlayer.Pause();
        mediaPlayer.Dispose();
        mediaPlayer = null;
    }

    public void Restart()
    {
        if(mediaPlayer is null)
        {
            return;
        }

        mediaPlayer.Position = default;
        mediaPlayer.Play();
    }

    private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
    {
        Restart();
    }

    public override void Dispose()
    {
        base.Dispose();

        mediaPlayer?.Dispose();
        mediaPlayer = null;
    }

}
