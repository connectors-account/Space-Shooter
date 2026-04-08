using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all game sound effects and music.
/// Singleton pattern for global access.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Sound Effects")]
    public AudioClip playerShootClip;
    public AudioClip enemyShootClip;
    public AudioClip playerHitClip;
    public AudioClip enemyDeathClip;
    public AudioClip playerDeathClip;
    public AudioClip powerUpClip;
    public AudioClip shieldBreakClip;
    public AudioClip gameOverClip;
    public AudioClip buttonClickClip;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;
    [Range(0f, 1f)]
    public float musicVolume = 0.4f;

    private Dictionary<string, AudioClip> soundMap;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Maps string names to audio clips for easy access.
    /// </summary>
    private void InitializeSoundMap()
    {
        soundMap = new Dictionary<string, AudioClip>
        {
            { "PlayerShoot", playerShootClip },
            { "EnemyShoot", enemyShootClip },
            { "PlayerHit", playerHitClip },
            { "EnemyDeath", enemyDeathClip },
            { "PlayerDeath", playerDeathClip },
            { "PowerUp", powerUpClip },
            { "ShieldBreak", shieldBreakClip },
            { "GameOver", gameOverClip },
            { "ButtonClick", buttonClickClip }
        };
    }

    /// <summary>
    /// Plays a sound effect by name.
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (sfxSource == null) return;

        if (soundMap != null && soundMap.ContainsKey(soundName))
        {
            AudioClip clip = soundMap[soundName];
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }
    }

    /// <summary>
    /// Plays a specific AudioClip directly.
    /// </summary>
    public void PlayClip(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Starts playing background music.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
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
    /// Sets SFX volume.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// Sets music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
}
