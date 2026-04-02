// =============================================================================
// AudioManager.cs
// Centralized audio system that manages background music and sound effects.
// Uses a dictionary of named AudioClips for easy SFX playback from any script.
// This is a singleton that persists across scenes.
// Create an empty GameObject named "AudioManager" and attach this script.
// =============================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Serializable struct pairing a name with an AudioClip.
/// Used to configure sounds in the Unity Inspector.
/// </summary>
[System.Serializable]
public class SoundEffect
{
    [Tooltip("Unique name to identify this sound effect (e.g., 'PlayerShoot', 'Explosion').")]
    public string name;

    [Tooltip("The AudioClip to play for this sound effect.")]
    public AudioClip clip;

    [Tooltip("Volume of this sound effect (0 to 1).")]
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static AudioManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Audio Settings
    // -------------------------------------------------------------------------
    [Header("Background Music")]
    [Tooltip("The background music AudioClip.")]
    public AudioClip backgroundMusic;

    [Tooltip("Volume of the background music.")]
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;

    [Tooltip("Whether background music should loop.")]
    public bool loopMusic = true;

    [Header("Sound Effects")]
    [Tooltip("List of all sound effects in the game. Configure names and clips here.")]
    public SoundEffect[] soundEffects;

    [Tooltip("Master volume for all sound effects.")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private Dictionary<string, SoundEffect> sfxDictionary;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialize singleton, create audio sources, and build the SFX dictionary.
    /// </summary>
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create AudioSource for background music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = loopMusic;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        // Create AudioSource for sound effects
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        // Build dictionary for fast SFX lookups by name
        sfxDictionary = new Dictionary<string, SoundEffect>();
        if (soundEffects != null)
        {
            foreach (SoundEffect sfx in soundEffects)
            {
                if (!string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
                {
                    sfxDictionary[sfx.name] = sfx;
                }
            }
        }
    }

    /// <summary>
    /// Start playing background music when the game begins.
    /// </summary>
    void Start()
    {
        PlayMusic();
    }

    // -------------------------------------------------------------------------
    // Music Control
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts playing the background music. Does nothing if no clip is assigned.
    /// </summary>
    public void PlayMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stops the background music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Sets the background music volume.
    /// </summary>
    /// <param name="volume">Volume level (0 to 1).</param>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    // -------------------------------------------------------------------------
    // Sound Effect Playback
    // -------------------------------------------------------------------------

    /// <summary>
    /// Plays a sound effect by name. The name must match an entry in the
    /// soundEffects array configured in the Inspector.
    /// If the sound is not found, a warning is logged (no crash).
    /// </summary>
    /// <param name="sfxName">Name of the sound effect to play.</param>
    public void PlaySFX(string sfxName)
    {
        if (sfxDictionary == null) return;

        if (sfxDictionary.TryGetValue(sfxName, out SoundEffect sfx))
        {
            sfxSource.PlayOneShot(sfx.clip, sfx.volume * sfxVolume);
        }
        else
        {
            // Log a warning but don't crash — allows running without audio files
            Debug.LogWarning("AudioManager: Sound effect '" + sfxName + "' not found. " +
                             "Add it to the AudioManager's Sound Effects list in the Inspector.");
        }
    }

    /// <summary>
    /// Plays an AudioClip directly (bypasses the naming system).
    /// Useful for one-off sounds not in the dictionary.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="volume">Volume (0 to 1).</param>
    public void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume * sfxVolume);
        }
    }

    /// <summary>
    /// Sets the master volume for all sound effects.
    /// </summary>
    /// <param name="volume">Volume level (0 to 1).</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
