using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class SoundService : MacroService
{
    private Dictionary<string, SoundPlayer> SoundEffects;

    public SoundService(MacroProcessor processor) : base(processor)
    {
    }

    public void Beep()
    {
        SystemSounds.Beep.Play();
    }

    public void Asterisk()
    {
        SystemSounds.Asterisk.Play();
    }

    public void Exclamation()
    {
        SystemSounds.Exclamation.Play();
    }

    public void Hand()
    {
        SystemSounds.Hand.Play();
    }

    public void Question()
    {
        SystemSounds.Question.Play();
    }

    public SoundPlayer AddSfx(string path)
    {
        return AddSfx(path, path);
    }
    public SoundPlayer AddSfx(string key, string path)
    {
        SoundEffects ??= new();

        if (!SoundEffects.TryGetValue(key, out var player))
        {
            player = GetSoundPlayer(path);
            SoundEffects.Add(key, player);
        }

        return player;
    }

    private SoundPlayer GetSoundPlayer(string path)
    {
        if (path.StartsWith("pack://application:,,,/"))
        {
            try
            {
                var sri = Application.GetResourceStream(new Uri(path));
                var player = new SoundPlayer(sri.Stream);
                player.Load();
                sri.Stream.Close();
                Logger.Debug($"Loaded sound resource \"{path}\".");
                return player;
            }
            catch (IOException)
            {
                Logger.Warn($"Cannot find sound resource \"{path}\".");
                return null;
            }
        }
        else
        {
            var player = new SoundPlayer(path);
            try
            {
                player.Load();
                Logger.Debug($"Loaded sound file \"{path}\".");
            }
            catch (IOException)
            {
                Logger.Warn($"Cannot find sound file \"{path}\".");
            }
            catch (Exception e)
            {
                Logger.Warn($"Cannot load sound file \"{path}\": {e.Message}.");
            }
            return player;
        }
    }

    public void PlaySfx(string path)
    {
        var player = AddSfx(path);

        if(player != null)
        {
            try
            {
                player.PlaySync();
            }
            catch (InvalidOperationException)
            {
                Logger.Warn($"Cannot play sound effect \"{path}\". The file is not a valid .wav file.");
                SoundEffects[path] = null;
            }
        }
    }

    public void PlayBgm(string path)
    {
        throw new NotImplementedException();
    }
    
    public void StopBgm(int fadeout = 0)
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        base.Dispose();

        if(SoundEffects?.Count > 0)
        {
            foreach (var (_, sp) in SoundEffects)
            {
                sp?.Dispose();
            }
            SoundEffects.Clear();
        }
    }
}
