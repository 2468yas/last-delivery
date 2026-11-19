using System.Collections.Generic;
using UnityEngine;

// Plays sound effects and music from Assets/Resources/Audio by file name.
// No setup needed: the first call creates it, and it survives scene changes.
//   AudioManager.Play("knock");
//   AudioManager.PlayMusic("music_calm");
public class AudioManager : MonoBehaviour
{
    static AudioManager instance;

    AudioSource sfx;
    AudioSource music;
    readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static float MusicVolume = 0.35f;

    static AudioManager Get()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AudioManager>();
            instance.sfx = go.AddComponent<AudioSource>();
            instance.music = go.AddComponent<AudioSource>();
            instance.music.loop = true;
            instance.music.volume = MusicVolume;
        }
        return instance;
    }

    public static void Play(string name, float volume = 1f)
    {
        AudioManager a = Get();
        AudioClip clip = a.Load(name);
        if (clip != null) a.sfx.PlayOneShot(clip, volume);
    }

    // keeps playing if the same track is already on (no restart between scenes)
    public static void PlayMusic(string name)
    {
        AudioManager a = Get();
        AudioClip clip = a.Load(name);
        if (clip == null || (a.music.clip == clip && a.music.isPlaying)) return;
        a.music.clip = clip;
        a.music.volume = MusicVolume;
        a.music.Play();
    }

    public static void StopMusic()
    {
        if (instance != null) instance.music.Stop();
    }

    AudioClip Load(string name)
    {
        if (!cache.TryGetValue(name, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip == null) Debug.LogWarning("AudioManager: no clip at Resources/Audio/" + name);
            cache[name] = clip;
        }
        return clip;
    }
}
