// ============================================================================
// AudioManager.cs - Centralized sound effects management (Singleton)
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioManager handles all sound effects and background music.
/// Uses a dictionary of named clips for easy access from any script.
/// Access via AudioManager.Instance.PlaySFX("ClipName").
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ---- Singleton ----
    public static AudioManager Instance { get; private set; }

    // ---- Audio Sources ----
    [Header("Audio Sources")]
    [Tooltip("AudioSource for background music")]
    public AudioSource musicSource;

    [Tooltip("AudioSource for sound effects (can overlap)")]
    public AudioSource sfxSource;

    // ---- Sound Effect Clips ----
    [Header("Sound Effects")]
    [Tooltip("Named sound effect clips")]
    public SoundEffect[] soundEffects;

    // ---- Volume ----
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    // ---- Internal ----
    private Dictionary<string, AudioClip> _sfxDictionary = new Dictionary<string, AudioClip>();

    // ---- Serializable struct for Inspector ----
    [Serializable]
    public struct SoundEffect
    {
        [Tooltip("Name used to reference this clip in code (e.g., 'PlayerShoot', 'Explosion')")]
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volumeScale;
    }

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build the dictionary for fast lookup
        foreach (var sfx in soundEffects)
        {
            if (!string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
            {
                _sfxDictionary[sfx.name] = sfx.clip;
            }
        }

        // Create audio sources if not assigned
        EnsureAudioSources();
    }

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>Play a named sound effect.</summary>
    public void PlaySFX(string clipName)
    {
        if (sfxSource == null) return;

        if (_sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            float volume = sfxVolume * masterVolume;

            // Find specific volume scale
            foreach (var sfx in soundEffects)
            {
                if (sfx.name == clipName)
                {
                    volume *= sfx.volumeScale > 0 ? sfx.volumeScale : 1f;
                    break;
                }
            }

            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            // Silently ignore missing clips (common during development)
            // Uncomment for debugging:
            // Debug.LogWarning($"AudioManager: Clip '{clipName}' not found.");
        }
    }

    /// <summary>Play background music (loops).</summary>
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null || musicClip == null) return;

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    /// <summary>Stop background music.</summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    /// <summary>Set master volume (0–1).</summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>Set SFX volume (0–1).</summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    /// <summary>Set music volume (0–1).</summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }

    // ========================================================================
    // Internal Helpers
    // ========================================================================
    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }
}
