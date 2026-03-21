using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio manager. Plays sound effects and background music.
/// Singleton that persists across scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Sound Effects")]
    public SFXClip[] sfxClips;

    [System.Serializable]
    public class SFXClip
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.8f, 1.2f)] public float pitchVariation = 1f;
    }

    private Dictionary<string, SFXClip> sfxDictionary = new Dictionary<string, SFXClip>();

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build lookup dictionary
        foreach (var clip in sfxClips)
        {
            if (!sfxDictionary.ContainsKey(clip.name))
                sfxDictionary[clip.name] = clip;
        }

        // Create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    /// <summary>Play a named sound effect.</summary>
    public void PlaySFX(string sfxName)
    {
        if (!sfxDictionary.TryGetValue(sfxName, out SFXClip sfx)) return;
        if (sfx.clip == null) return;

        float pitch = Random.Range(2f - sfx.pitchVariation, sfx.pitchVariation);
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(sfx.clip, sfx.volume * sfxVolume * masterVolume);
    }

    /// <summary>Play background music for the menu.</summary>
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    /// <summary>Play background music for gameplay.</summary>
    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    /// <summary>Stop all music.</summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>Update master volume.</summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>Update music volume.</summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>Update SFX volume.</summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
