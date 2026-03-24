using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager provides a central point for playing sound effects and background music.
/// SFX clips are registered in a dictionary by name so any script can trigger them.
/// Persists across scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ── Audio Sources ────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // ── Music Clips ──────────────────────────────────────────
    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    // ── SFX Clips ────────────────────────────────────────────
    [Header("Sound Effects")]
    [SerializeField] private AudioClip playerShootClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip shieldHitClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip waveStartClip;

    // ── Volume ───────────────────────────────────────────────
    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    // ── Internal SFX lookup ──────────────────────────────────
    private Dictionary<string, AudioClip> sfxClips;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton – persists across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create AudioSources if not assigned
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

        // Build the SFX dictionary
        sfxClips = new Dictionary<string, AudioClip>
        {
            { "PlayerShoot", playerShootClip },
            { "Explosion",   explosionClip },
            { "PlayerHit",   playerHitClip },
            { "ShieldHit",   shieldHitClip },
            { "PowerUp",     powerUpClip },
            { "ButtonClick", buttonClickClip },
            { "WaveStart",   waveStartClip }
        };

        ApplyVolumes();
    }

    // ──────────────────────────────────────────────────────────
    // Music
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Play the menu background music loop.
    /// </summary>
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    /// <summary>
    /// Play the gameplay background music loop.
    /// </summary>
    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    /// <summary>
    /// Stop the current music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;
        if (clip == null) return;

        // Don't restart the same track
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // ──────────────────────────────────────────────────────────
    // SFX
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Play a sound effect by name. Names must match dictionary keys:
    /// "PlayerShoot", "Explosion", "PlayerHit", "ShieldHit",
    /// "PowerUp", "ButtonClick", "WaveStart".
    /// Gracefully does nothing if the clip is null (no audio assigned).
    /// </summary>
    public void PlaySFX(string clipName)
    {
        if (sfxSource == null) return;

        if (sfxClips.TryGetValue(clipName, out AudioClip clip))
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: SFX clip '{clipName}' not found in dictionary.");
        }
    }

    /// <summary>
    /// Play an arbitrary AudioClip as SFX.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ──────────────────────────────────────────────────────────
    // Volume Control
    // ──────────────────────────────────────────────────────────

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
    }

    private void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
    }
}
