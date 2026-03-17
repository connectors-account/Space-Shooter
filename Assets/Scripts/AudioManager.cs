using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Centralized audio management with named sound effects and background music.
/// Persists across scenes using DontDestroyOnLoad.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffect[] soundEffects;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;

    // Dictionary for quick SFX lookup
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build SFX dictionary
        if (soundEffects != null)
        {
            foreach (SoundEffect sfx in soundEffects)
            {
                if (!string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
                {
                    sfxDictionary[sfx.name] = sfx.clip;
                }
            }
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
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Play a sound effect by name. If the clip is not found, the call is silently ignored.
    /// </summary>
    /// <param name="sfxName">Name of the sound effect as defined in the soundEffects array.</param>
    public void PlaySFX(string sfxName)
    {
        if (sfxDictionary.TryGetValue(sfxName, out AudioClip clip))
        {
            sfxSource.volume = sfxVolume * masterVolume;
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            // SFX not found - this is expected during development with placeholder assets
            // Debug.LogWarning($"SFX '{sfxName}' not found in AudioManager.");
        }
    }

    /// <summary>
    /// Play background music for the menu.
    /// </summary>
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    /// <summary>
    /// Play background music for gameplay.
    /// </summary>
    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    /// <summary>
    /// Play a music clip, replacing the current one.
    /// </summary>
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Stop the currently playing music.
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Set the master volume (0-1).
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>
    /// Set the music volume (0-1).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>
    /// Set the SFX volume (0-1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}

/// <summary>
/// Serializable sound effect entry mapping a name to an AudioClip.
/// </summary>
[Serializable]
public struct SoundEffect
{
    public string name;
    public AudioClip clip;
}
