using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all game audio: music and sound effects.
/// Singleton - persists across scenes.
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
    [SerializeField] private AudioClip playerShootClip;
    [SerializeField] private AudioClip enemyShootClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip enemyExplosionClip;
    [SerializeField] private AudioClip playerExplosionClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip shieldActivateClip;
    [SerializeField] private AudioClip shieldBreakClip;
    [SerializeField] private AudioClip weaponUpgradeClip;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip waveStartClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    private Dictionary<string, AudioClip> sfxMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup audio sources if not assigned
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

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        BuildSFXMap();
    }

    private void BuildSFXMap()
    {
        sfxMap = new Dictionary<string, AudioClip>
        {
            { "PlayerShoot", playerShootClip },
            { "EnemyShoot", enemyShootClip },
            { "PlayerHit", playerHitClip },
            { "EnemyExplosion", enemyExplosionClip },
            { "PlayerExplosion", playerExplosionClip },
            { "PowerUp", powerUpClip },
            { "ShieldActivate", shieldActivateClip },
            { "ShieldBreak", shieldBreakClip },
            { "WeaponUpgrade", weaponUpgradeClip },
            { "Heal", healClip },
            { "ButtonClick", buttonClickClip },
            { "WaveStart", waveStartClip },
            { "GameOver", gameOverClip }
        };
    }

    /// <summary>
    /// Play a sound effect by name.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (sfxMap != null && sfxMap.TryGetValue(sfxName, out AudioClip clip))
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }
    }

    /// <summary>
    /// Play a sound effect clip directly.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}
