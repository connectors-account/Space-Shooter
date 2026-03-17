using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple audio manager for SFX and music playback.
/// Attach to a persistent AudioManager GameObject.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioClip playerShoot;
    public AudioClip enemyShoot;
    public AudioClip playerHit;
    public AudioClip enemyExplosion;
    public AudioClip playerExplosion;
    public AudioClip powerUpPickup;
    public AudioClip shieldBreak;

    private Dictionary<string, AudioClip> sfxMap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSFXMap();

        // Create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.3f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = 0.5f;
        }
    }

    void Start()
    {
        PlayMusic();
    }

    void InitializeSFXMap()
    {
        sfxMap = new Dictionary<string, AudioClip>
        {
            { "PlayerShoot", playerShoot },
            { "EnemyShoot", enemyShoot },
            { "PlayerHit", playerHit },
            { "EnemyExplosion", enemyExplosion },
            { "PlayerExplosion", playerExplosion },
            { "PowerUp", powerUpPickup },
            { "ShieldBreak", shieldBreak }
        };
    }

    public void PlayMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource?.Stop();
    }

    /// <summary>
    /// Play a named sound effect. Silently ignores if clip is null.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (sfxSource == null) return;
        if (sfxMap == null) return;

        if (sfxMap.TryGetValue(sfxName, out AudioClip clip))
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }
}
