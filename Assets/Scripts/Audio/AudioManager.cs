using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all game audio: background music and sound effects.
/// Singleton pattern. SFX are played through a pool of AudioSources.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int sfxPoolSize = 10;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip playerShootClip;
    [SerializeField] private AudioClip enemyShootClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip enemyExplosionClip;
    [SerializeField] private AudioClip playerExplosionClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip shieldBreakClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip menuSelectClip;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.5f;

    private AudioSource[] sfxSources;
    private int currentSfxIndex = 0;
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

        InitializeSFXPool();
        BuildSFXMap();
    }

    private void Start()
    {
        PlayMusic();
    }

    private void InitializeSFXPool()
    {
        sfxSources = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject go = new GameObject("SFX_Source_" + i);
            go.transform.parent = transform;
            sfxSources[i] = go.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
            sfxSources[i].volume = sfxVolume;
        }
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
            { "ShieldBreak", shieldBreakClip },
            { "GameOver", gameOverClip },
            { "MenuSelect", menuSelectClip }
        };
    }

    /// <summary>
    /// Play a named sound effect from the pool.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (sfxMap == null) return;
        if (!sfxMap.ContainsKey(sfxName)) return;

        AudioClip clip = sfxMap[sfxName];
        if (clip == null) return; // clip not assigned, gracefully skip

        sfxSources[currentSfxIndex].clip = clip;
        sfxSources[currentSfxIndex].volume = sfxVolume;
        sfxSources[currentSfxIndex].Play();

        currentSfxIndex = (currentSfxIndex + 1) % sfxPoolSize;
    }

    /// <summary>
    /// Play or restart background music.
    /// </summary>
    public void PlayMusic()
    {
        if (musicSource == null || backgroundMusic == null) return;

        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = vol;
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = vol;
    }
}
