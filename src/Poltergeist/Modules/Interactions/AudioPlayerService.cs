using Poltergeist.Automations.Components.Interactions;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Poltergeist.Modules.Interactions;

public class AudioPlayerService
{
    private MediaPlayer? MediaPlayer;

    public AudioPlayerService()
    {
    }

    private void ReleasePlayer()
    {
        MediaPlayer?.MediaEnded -= MediaPlayer_MediaEnded;
        MediaPlayer?.Dispose();
        MediaPlayer = null;
    }

    public void Play(string path)
    {
        PlayInternal(path, false);
    }

    public void Play(AudioModel model)
    {
        PlayInternal(model.FilePath, model.IsLooping);

        if (model.Duration != default)
        {
            var player = MediaPlayer;
            Task.Delay(model.Duration).ContinueWith(_ =>
            {
                if (player != MediaPlayer)
                {
                    return;
                }

                Stop();
            });
        }
    }
    
    public void Stop()
    {
        if (MediaPlayer is null)
        {
            return;
        }

        MediaPlayer.Pause();
        ReleasePlayer();
    }

    public void Restart()
    {
        if (MediaPlayer is null)
        {
            return;
        }

        MediaPlayer.Position = default;
        MediaPlayer.Play();
    }

    private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
    {
        Restart();
    }

    private void PlayInternal(string path, bool isLooping)
    {
        ReleasePlayer();

        MediaPlayer = new MediaPlayer
        {
            Source = MediaSource.CreateFromUri(new Uri(path)),
        };
        if (isLooping)
        {
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        MediaPlayer.Play();
    }
}
