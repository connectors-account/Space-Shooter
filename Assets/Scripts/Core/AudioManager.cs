// ============================================================================
// AudioManager.cs - Centralized audio playback (Singleton)
// Manages background music and one-shot sound effects with volume control.
// ============================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton AudioManager that handles all game audio: music and SFX.
/// Loads AudioClips from Resources/Audio/ at runtime so designers can drop
/// new clips into the folder without touching code.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ---- Singleton ----
    public static AudioManager Instance { get; private set; }

    // ---- Audio Sources ----
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // ---- Volume ----
    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.7f;

    // ---- Clip Cache ----
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    // ---- Sound Effect Name Constants ----
    /// <summary>Standard SFX names used throughout the game.</summary>
    public static class SFX
    {
        public const string PlayerShoot   = "player_shoot";
        public const string EnemyShoot    = "enemy_shoot";
        public const string Explosion     = "explosion";
        public const string PowerUp       = "powerup";
        public const string Hit           = "hit";
        public const string ShieldUp      = "shield_up";
        public const string ShieldHit     = "shield_hit";
        public const string WeaponUpgrade = "weapon_upgrade";
        public const string UIClick       = "ui_click";
        public const string WaveStart     = "wave_start";
        public const string GameOverSting = "game_over";
        public const string ComboUp       = "combo_up";
    }

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources programmatically if not assigned in the Inspector.
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }

        // Pre-load any clips in Resources/Audio.
        PreloadClips();
    }

    // ========================================================================
    // Music
    // ========================================================================

    /// <summary>
    /// Plays background music by clip name (loaded from Resources/Audio/).
    /// </summary>
    /// <param name="clipName">File name without extension inside Resources/Audio/Music/.</param>
    public void PlayMusic(string clipName)
    {
        AudioClip clip = GetClip("Audio/Music/" + clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] Music clip '{clipName}' not found in Resources/Audio/Music/.");
            return;
        }
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>Stops the currently playing music track.</summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>Sets the music volume (0–1) and applies it immediately.</summary>
    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        musicSource.volume = musicVolume;
    }

    // ========================================================================
    // Sound Effects
    // ========================================================================

    /// <summary>
    /// Plays a one-shot sound effect by name from Resources/Audio/SFX/.
    /// Safe to call rapidly; overlapping clips are fine.
    /// </summary>
    /// <param name="clipName">File name without extension inside Resources/Audio/SFX/.</param>
    public void PlaySFX(string clipName)
    {
        AudioClip clip = GetClip("Audio/SFX/" + clipName);
        if (clip == null)
        {
            // Silently ignore missing SFX so the game runs without audio assets.
            return;
        }
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Plays a one-shot SFX with a custom volume multiplier (0–1).
    /// </summary>
    public void PlaySFX(string clipName, float volumeScale)
    {
        AudioClip clip = GetClip("Audio/SFX/" + clipName);
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    /// <summary>Sets the global SFX volume (0–1).</summary>
    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        sfxSource.volume = sfxVolume;
    }

    // ========================================================================
    // Internals
    // ========================================================================

    /// <summary>
    /// Pre-loads all AudioClips found in Resources/Audio into the cache.
    /// </summary>
    private void PreloadClips()
    {
        AudioClip[] allClips = Resources.LoadAll<AudioClip>("Audio");
        foreach (AudioClip clip in allClips)
        {
            if (!clipCache.ContainsKey(clip.name))
            {
                clipCache[clip.name] = clip;
            }
        }
    }

    /// <summary>
    /// Retrieves a clip from the cache or loads it from Resources on demand.
    /// </summary>
    private AudioClip GetClip(string resourcePath)
    {
        string key = System.IO.Path.GetFileNameWithoutExtension(resourcePath);
        if (clipCache.TryGetValue(key, out AudioClip cached))
        {
            return cached;
        }

        AudioClip loaded = Resources.Load<AudioClip>(resourcePath);
        if (loaded != null)
        {
            clipCache[key] = loaded;
        }
        return loaded;
    }
}
